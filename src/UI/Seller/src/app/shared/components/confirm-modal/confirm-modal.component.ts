import { Component, Output, EventEmitter, Input } from '@angular/core'

import { NgbModal } from '@ng-bootstrap/ng-bootstrap'

@Component({
  selector: 'confirm-modal',
  templateUrl: './confirm-modal.component.html',
})
export class ConfirmModal {
  @Input()
  modalTitle: string
  @Input()
  description: string
}
