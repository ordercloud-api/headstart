import { NgModule } from '@angular/core'
import { SharedModule } from '@app-seller/shared'
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar'
import { BuyersRoutingModule } from './buyers-routing.module'
import { BuyerTableComponent } from './components/buyers/buyer-table/buyer-table.component'
import { BuyerUserTableComponent } from './components/users/buyer-user-table/buyer-user-table.component'
import { BuyerLocationTableComponent } from './components/locations/buyer-location-table/buyer-location-table.component'
import { BuyerCategoryTableComponent } from './components/categories/buyer-category-table/buyer-category-table.component'
import { BuyerLocationEditComponent } from './components/locations/buyer-location-edit/buyer-location-edit.component'
import { BuyerCategoryEditComponent } from './components/categories/buyer-category-edit/buyer-category-edit.component'
import { BuyerUserEditComponent } from './components/users/buyer-user-edit/buyer-user-edit.component'
import { BuyerEditComponent } from './components/buyers/buyer-edit/buyer-edit.component'
import { BuyerLocationPermissions } from './components/locations/buyer-location-permissions/buyer-location-permissions'
import { BuyerCatalogTableComponent } from './components/catalogs/buyer-catalog-table/buyer-catalog-table.component'
import { BuyerLocationCatalogs } from './components/locations/buyer-location-catalogs/buyer-location-catalogs.component'
import { NgbModule, NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap'
import { BuyerLocationApprovals } from './components/locations/buyer-location-approvals/buyer-location-approvals.component'

@NgModule({
  imports: [SharedModule, 
    BuyersRoutingModule, 
    PerfectScrollbarModule,
    NgbModule,
    NgbTooltipModule],
  declarations: [
    BuyerTableComponent,
    BuyerEditComponent,
    BuyerCategoryTableComponent,
    BuyerCategoryEditComponent,
    BuyerCatalogTableComponent,
    BuyerCategoryEditComponent,
    BuyerLocationTableComponent,
    BuyerLocationPermissions,
    BuyerLocationCatalogs,
    BuyerLocationEditComponent,
    BuyerUserTableComponent,
    BuyerUserEditComponent,
    BuyerLocationApprovals
  ],
})
export class BuyersModule {}
