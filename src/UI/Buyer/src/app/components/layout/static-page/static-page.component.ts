import { Component, OnInit, Input } from '@angular/core'

@Component({
  templateUrl: './static-page.component.html',
  styleUrls: ['./static-page.component.scss'],
})
export class OCMStaticPage implements OnInit {
  @Input() page: any // TODO: add PageDocument type to cms library so this is strongly typed

  constructor() {}

  ngOnInit(): void {}
}
