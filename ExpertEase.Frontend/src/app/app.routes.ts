import { Routes } from '@angular/router';
import {HomeComponent} from './pages/home/home.component';
import {AboutComponent} from './pages/about/about.component';
import {NotFoundComponent} from './pages/not-found/not-found.component';
import {SolicitationsComponent} from './pages/solicitations/solicitations.component';
import {AccountComponent} from './pages/account/account.component';
import {RegisterComponent} from './pages/auth/register/register.component';
import {LoginComponent} from './pages/auth/login/login.component';
import {AdminComponent} from './pages/admin/admin.component';
import {AdminUsersComponent} from './pages/admin/admin.users/admin.users.component';
import {AdminSpecialistsComponent} from './pages/admin/admin.specialists/admin.specialists.component';

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
    path: 'solicitations',
    component: SolicitationsComponent
  },
  {
    path: 'account',
    component: AccountComponent
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
    path: 'admin',
    component: AdminComponent,
  },
  {
    path: 'admin/users',
    component: AdminUsersComponent,
  },
  {
    path: 'admin/specialists',
    component: AdminSpecialistsComponent,
  },
  {
    path: '',
    redirectTo: '/home',
    pathMatch: 'full'
  },
  {
    path: '**',
    component: NotFoundComponent
  },
];
