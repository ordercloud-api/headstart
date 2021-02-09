// core services
import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { ProductTableComponent } from './components/product-table/product-table.component'
import { IsSellerGuard } from '@app-seller/shared/guards/is-seller/is-seller.guard'

const routes: Routes = [
  { path: '', component: ProductTableComponent, pathMatch: 'prefix' },
  { path: 'new/standard', component: ProductTableComponent, pathMatch: 'full' },
  { path: 'new/quote', component: ProductTableComponent, pathMatch: 'full' },
  {
    path: 'new/purchase-order',
    component: ProductTableComponent,
    pathMatch: 'full',
  },
  { path: ':productID', component: ProductTableComponent, pathMatch: 'full' },
  {
    path: ':productID/catalog-assignments',
    component: ProductTableComponent,
    pathMatch: 'full',
    canActivate: [IsSellerGuard],
  },
  {
    path: ':productID/description',
    component: ProductTableComponent,
    pathMatch: 'full',
  },
  {
    path: ':productID/price',
    component: ProductTableComponent,
    pathMatch: 'full',
  },
  {
    path: ':productID/filters',
    component: ProductTableComponent,
    pathMatch: 'full',
  },
  {
    path: ':productID/variants',
    component: ProductTableComponent,
    pathMatch: 'full',
  },
  {
    path: ':productID/images-and-documents',
    component: ProductTableComponent,
    pathMatch: 'full',
  },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ProductsRoutingModule {}
