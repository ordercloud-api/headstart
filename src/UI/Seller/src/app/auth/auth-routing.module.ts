// core services
import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'

// auth components
import { LoginComponent } from '@app-seller/auth/containers/login/login.component'
import { ForgotPasswordComponent } from '@app-seller/auth/containers/forgot-password/forgot-password.component'
import { ResetPasswordComponent } from '@app-seller/auth/containers/reset-password/reset-password.component'

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AuthRoutingModule {}
