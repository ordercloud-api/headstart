import { SafeUrl } from '@angular/platform-browser'

export interface FileHandle {
  File: File
  URL: SafeUrl
  Filename?: string
}
