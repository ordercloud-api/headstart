import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { RMATableComponent } from './components/rmas-table/rmas-table.component'

const routes: Routes = [
  { path: '', component: RMATableComponent },
  { path: 'new', component: RMATableComponent, pathMatch: 'full' },
  { path: ':RMANumber', component: RMATableComponent, pathMatch: 'full' },
]
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class RMARoutingModule {}
