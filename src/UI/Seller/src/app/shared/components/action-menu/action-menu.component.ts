import { Component, Input, Output, EventEmitter } from '@angular/core'
import { getSupportedInputTypes } from '@angular/cdk/platform'

@Component({
  selector: 'action-menu-component',
  templateUrl: './action-menu.component.html',
  styleUrls: ['./action-menu.component.scss'],
})
export class ActionMenuComponent {
  @Input()
  dataIsSaving = false
  @Input()
  isCreatingNew = false
  @Input()
  areChanges = false
  @Input()
  allowDiscard = false
  @Input()
  allowDelete = false
  @Input()
  isDeletable = false
  @Input()
  saveTextOverride = ''
  @Input()
  resourceName = ''
  @Input()
  requireConfirmation = false
  @Input()
  disableSave = false
  @Input()
  confirmText = ''
  @Input()
  labelSingular: string
  @Output()
  executeSaveAction = new EventEmitter<void>()
  @Output()
  executeDiscardAction = new EventEmitter<void>()
  @Output()
  executeDelete = new EventEmitter<void>()

  showConfirm = false

  requestUserConfirmation(): void {
    this.showConfirm = true
  }

  emitSave(): void {
    this.executeSaveAction.emit()
  }

  handleSavePressed(): void {
    if (this.showConfirm) {
      this.requestUserConfirmation()
    } else {
      this.emitSave()
    }
  }

  handleDiscard(): void {
    this.executeDiscardAction.emit()
  }

  getSaveText(): string {
    if (this.dataIsSaving) {
      return 'ADMIN.COMMON.SAVING'
    }
    if (this.showConfirm) {
      return 'ADMIN.DELETE.PENDING_CONFIRMATION'
    }
    if (this.saveTextOverride) {
      return this.saveTextOverride
    }
    return this.isCreatingNew
      ? 'ADMIN.COMMON.CREATE'
      : 'ADMIN.COMMON.SAVE_CHANGES'
  }

  handleDelete(): void {
    this.executeDelete.emit()
  }
}
