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
import {SpecialistDetailsComponent} from './shared/specialist-details/specialist-details.component';
import {RequestFormComponent} from './shared/request-form/request-form.component';

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
    path: 'messages',
    component: MessagesComponent
  },
  {
    path: 'profile',
    component: ProfileComponent
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
    path: '',
    redirectTo: '/home',
    pathMatch: 'full'
  },
  {
    path: '**',
    component: UnauthorizedComponent
  },
];
