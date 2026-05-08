use std::sync::Mutex;
use tauri::Manager;
use tauri_plugin_shell::process::CommandChild;
use tauri_plugin_shell::ShellExt;

#[allow(dead_code)]
struct ApiSidecar(Mutex<CommandChild>);

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

            let (_rx, child) = app
                .shell()
                .sidecar("JobTracker.Api")?
                .env("ASPNETCORE_URLS", "http://localhost:5063")
                .env("ASPNETCORE_ENVIRONMENT", "Development")
                .spawn()?;
            app.manage(ApiSidecar(Mutex::new(child)));

            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
