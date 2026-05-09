!macro NSIS_HOOK_PREINSTALL
  nsExec::Exec 'taskkill /F /IM "JobTracker.Api.exe" /T'
  nsExec::Exec 'taskkill /F /IM "job-tracker.exe" /T'
!macroend

!macro NSIS_HOOK_PREUNINSTALL
  nsExec::Exec 'taskkill /F /IM "JobTracker.Api.exe" /T'
  nsExec::Exec 'taskkill /F /IM "job-tracker.exe" /T'
!macroend
