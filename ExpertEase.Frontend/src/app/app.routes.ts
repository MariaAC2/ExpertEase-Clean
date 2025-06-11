import { Routes } from '@angular/router';
import {HomeComponent} from './pages/home/home.component';
import {AboutComponent} from './pages/about/about.component';
import {RegisterComponent} from './pages/auth/register/register.component';
import {LoginComponent} from './pages/auth/login/login.component';
import {AdminComponent} from './pages/admin/admin.component';
import {AdminUsersComponent} from './pages/admin/admin.users/admin.users.component';
import {AdminSpecialistsComponent} from './pages/admin/admin.specialists/admin.specialists.component';
import {RoleGuard} from './pages/auth/role.guard';
import {UnauthorizedComponent} from './pages/unauthorized/unauthorized.component';
import {AuthGuard} from './pages/auth/auth.guard';
import {ProfileComponent} from './pages/profile/profile.component';
import {MessagesComponent} from './pages/messages/messages.component';
import {BecomeSpecialistComponent} from './pages/become-specialist/become-specialist.component';
import {SpecialistDetailsComponent} from './shared/specialist-details/specialist-details.component';
import {RequestFormComponent} from './shared/request-form/request-form.component';
import {AdminCategoriesComponent} from './pages/admin/admin.categories/admin.categories.component';
import {SettingsComponent} from './pages/profile/settings/settings.component';
import {ReviewsComponent} from './pages/profile/reviews/reviews.component';
import {ServicePaymentComponent} from './shared/service-payment/service-payment.component';
import {ContactUsComponent} from './pages/contact-us/contact-us.component';
import {TermsComponent} from './pages/terms/terms.component';
import {PrivacyComponent} from './pages/privacy/privacy.component';

export const routes: Routes = [
  {
    path: 'home',
    component: HomeComponent
  },
  {
    path: 'about',
    component: AboutComponent
  },
  {
    path: 'contact-us',
    component: ContactUsComponent
  },
  {
    path: 'terms',
    component: TermsComponent
  },
  {
    path: 'privacy',
    component: PrivacyComponent
  },
  {
    path: 'profile',
    component: ProfileComponent,
    // canActivate: [AuthGuard]
  },
  {
    path: 'home/:id',
    loadComponent: () =>
      import('./shared/specialist-details/specialist-details.component')
        .then(m => m.SpecialistDetailsComponent)
  },
  {
    path: 'specialist/:id',
    component: SpecialistDetailsComponent
  },
  {
    path: 'messages',
    component: MessagesComponent,
    // canActivate: [AuthGuard]
  },
  {
    path: 'profile/user/become-specialist',
    component: BecomeSpecialistComponent,
    // canActivate: [AuthGuard]
  },
  {
    path: 'profile/user-settings',
    component: SettingsComponent,
    // canActivate: [AuthGuard]
  },
  {
    path: 'profile/user-reviews',
    component: ReviewsComponent,
    // canActivate: [AuthGuard]
  },
  {
    path: 'request-form',
    component: RequestFormComponent
  },
  {
    path: 'register',
    component: RegisterComponent
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'specialist-details',
    component: SpecialistDetailsComponent
  },
  {
    path: 'service-payment',
    component: ServicePaymentComponent,
  },
  {
    path: 'admin',
    // canActivate: [AuthGuard, RoleGuard],
    component: AdminComponent,
    // data: { roles: ['Admin'] }
  },
  {
    path: 'admin/users',
    // canActivate: [AuthGuard, RoleGuard],
    component: AdminUsersComponent,
    // data: { roles: ['Admin'] }
  },
  {
    path: 'admin/specialists',
    // canActivate: [AuthGuard, RoleGuard],
    component: AdminSpecialistsComponent,
    // data: { roles: ['Admin'] }
  },
  {
    path: 'admin/categories',
    // canActivate: [AuthGuard, RoleGuard],
    component: AdminCategoriesComponent,
    // data: { roles: ['Admin'] }
  },
  {
    path: '',
    redirectTo: '/home',
    pathMatch: 'full'
  },
  {
    path: '**',
    component: UnauthorizedComponent
  },
];
