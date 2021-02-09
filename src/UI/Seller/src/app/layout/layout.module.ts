import { NgModule } from '@angular/core'
import { HeaderComponent } from '@app-seller/layout/header/header.component'
import { HomeComponent } from '@app-seller/layout/home/home.component'
import { SharedModule } from '@app-seller/shared'
import { RouterModule } from '@angular/router'
import { FormsModule } from '@angular/forms'

@NgModule({
  imports: [RouterModule, SharedModule, FormsModule],
  exports: [HeaderComponent, HomeComponent],
  declarations: [HeaderComponent, HomeComponent],
})
export class LayoutModule {}
