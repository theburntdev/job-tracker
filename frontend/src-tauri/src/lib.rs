use std::sync::Mutex;
use tauri::Manager;
use tauri_plugin_shell::process::{CommandChild, CommandEvent};
use tauri_plugin_shell::ShellExt;

struct ApiSidecar(Mutex<Option<CommandChild>>);

impl Drop for ApiSidecar {
    fn drop(&mut self) {
        if let Ok(mut guard) = self.0.lock() {
            if let Some(child) = guard.take() {
                let _ = child.kill();
            }
        }
    }
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_shell::init())
        .setup(|app| {
            if cfg!(debug_assertions) {
                app.handle().plugin(
                    tauri_plugin_log::Builder::default()
                        .level(log::LevelFilter::Info)
                        .build(),
                )?;
            }

            // Kill any leftover sidecar from a previous session
            let _ = std::process::Command::new("taskkill")
                .args(["/F", "/IM", "JobTracker.Api.exe", "/T"])
                .output();

            let data_dir = app.path().app_data_dir()?;
            std::fs::create_dir_all(&data_dir)?;
            let db_path = data_dir.join("jobtracker.db");

            let (mut rx, child) = app
                .shell()
                .sidecar("JobTracker.Api")?
                .env("ASPNETCORE_URLS", "http://localhost:5063")
                .env(
                    "ASPNETCORE_ENVIRONMENT",
                    if cfg!(debug_assertions) { "Development" } else { "Production" },
                )
                .env("DB_PATH", db_path.to_str().unwrap_or("jobtracker.db"))
                .spawn()?;
            app.manage(ApiSidecar(Mutex::new(Some(child))));

            // Forward sidecar stdout/stderr to a log file in app data dir
            let log_path = data_dir.join("sidecar.log");
            tauri::async_runtime::spawn(async move {
                use std::io::Write;
                while let Some(event) = rx.recv().await {
                    let line = match &event {
                        CommandEvent::Stdout(b) => format!("[out] {}\n", String::from_utf8_lossy(b)),
                        CommandEvent::Stderr(b) => format!("[err] {}\n", String::from_utf8_lossy(b)),
                        CommandEvent::Terminated(s) => format!("[terminated] {:?}\n", s),
                        _ => continue,
                    };
                    log::info!("{}", line.trim());
                    if let Ok(mut f) = std::fs::OpenOptions::new().create(true).append(true).open(&log_path) {
                        let _ = f.write_all(line.as_bytes());
                    }
                    if matches!(event, CommandEvent::Terminated(_)) { break; }
                }
            });

            // Kill the sidecar when the terminal sends Ctrl+C (Rust's default exit skips Drop)
            let handle = app.handle().clone();
            ctrlc::set_handler(move || {
                if let Ok(mut guard) = handle.state::<ApiSidecar>().0.lock() {
                    if let Some(c) = guard.take() {
                        let _ = c.kill();
                    }
                }
                std::process::exit(0);
            })
            .ok();

            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
