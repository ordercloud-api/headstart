import { Component, OnInit, Input } from '@angular/core'
import { DocumentAsset } from '@ordercloud/headstart-sdk'

@Component({
  // ocm-product-attachments
  templateUrl: './product-attachments.component.html',
  styleUrls: ['./product-attachments.component.scss'],
})
export class OCMProductAttachments implements OnInit {
  @Input() attachments: DocumentAsset[]

  constructor() {}

  ngOnInit(): void {}
}
