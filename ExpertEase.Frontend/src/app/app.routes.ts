import { Routes } from '@angular/router';
import {HomeComponent} from './pages/home/home.component';
import {AboutComponent} from './pages/about/about.component';
import {NotFoundComponent} from './pages/not-found/not-found.component';
import {SolicitationsComponent} from './pages/solicitations/solicitations.component';
import {AccountComponent} from './pages/account/account.component';

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
    path: '',
    redirectTo: '/home',
    pathMatch: 'full'
  },
  {
    path: '**',
    component: NotFoundComponent
  },
];
