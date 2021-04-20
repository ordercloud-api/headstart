import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core'
import { faHeart as _faHeartFilled } from '@fortawesome/free-solid-svg-icons'
import { faHeart as _faHeartOutline } from '@fortawesome/free-regular-svg-icons'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './toggle-favorite.component.html',
  styleUrls: ['./toggle-favorite.component.scss'],
})
export class OCMToggleFavorite implements OnInit {
  faHeartFilled = _faHeartFilled
  faHeartOutline = _faHeartOutline
  @Input() favorite: boolean
  @Output() favoriteChanged = new EventEmitter<boolean>()
  isAnonymous: boolean

  constructor(
    private context: ShopperContextService
  ){ }

  ngOnInit() {
    this.isAnonymous = this.context.currentUser.isAnonymous()
  }
}
