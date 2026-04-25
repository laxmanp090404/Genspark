import { Routes } from '@angular/router';
import { ShellComponent } from './layout/shell.component';
import { authGuard } from './core/auth.guard';
import { roleGuard } from './core/role.guard';
import { LoginPageComponent } from './pages/auth/login-page.component';
import { SignupPageComponent } from './pages/auth/signup-page.component';
import { SearchPageComponent } from './pages/user/search-page.component';
import { BookBusPageComponent } from './pages/user/book-bus-page.component';
import { MyBookingsPageComponent } from './pages/user/my-bookings-page.component';
import { OperatorRequestPageComponent } from './pages/user/operator-request-page.component';
import { OperatorDashboardPageComponent } from './pages/operator/operator-dashboard-page.component';
import { OperatorRoutesPageComponent } from './pages/operator/operator-routes-page.component';
import { OperatorBusesPageComponent } from './pages/operator/operator-buses-page.component';
import { AdminDashboardPageComponent } from './pages/admin/admin-dashboard-page.component';
import { AdminRoutesPageComponent } from './pages/admin/admin-routes-page.component';
import { AdminOperatorRequestsPageComponent } from './pages/admin/admin-operator-requests-page.component';
import { AdminConfigPageComponent } from './pages/admin/admin-config-page.component';
import { AdminBookingsPageComponent } from './pages/admin/admin-bookings-page.component';

export const routes: Routes = [
  { path: 'login', component: LoginPageComponent },
  { path: 'signup', component: SignupPageComponent },
  {
    path: '',
    component: ShellComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'search' },
      { path: 'search', component: SearchPageComponent },
      { path: 'book/:scheduleId', component: BookBusPageComponent, canActivate: [authGuard, roleGuard], data: { roles: ['USER', 'BUS_OPERATOR', 'ADMIN'] } },
      { path: 'my-bookings', component: MyBookingsPageComponent, canActivate: [authGuard, roleGuard], data: { roles: ['USER'] } },
      { path: 'operator-request', component: OperatorRequestPageComponent, canActivate: [authGuard, roleGuard], data: { roles: ['USER'] } },
      { path: 'operator/dashboard', component: OperatorDashboardPageComponent, canActivate: [authGuard, roleGuard], data: { roles: ['BUS_OPERATOR'] } },
      { path: 'operator/routes', component: OperatorRoutesPageComponent, canActivate: [authGuard, roleGuard], data: { roles: ['BUS_OPERATOR'] } },
      { path: 'operator/buses', component: OperatorBusesPageComponent, canActivate: [authGuard, roleGuard], data: { roles: ['BUS_OPERATOR'] } },
      { path: 'admin/dashboard', component: AdminDashboardPageComponent, canActivate: [authGuard, roleGuard], data: { roles: ['ADMIN'] } },
      { path: 'admin/routes', component: AdminRoutesPageComponent, canActivate: [authGuard, roleGuard], data: { roles: ['ADMIN'] } },
      { path: 'admin/operator-requests', component: AdminOperatorRequestsPageComponent, canActivate: [authGuard, roleGuard], data: { roles: ['ADMIN'] } },
      { path: 'admin/config', component: AdminConfigPageComponent, canActivate: [authGuard, roleGuard], data: { roles: ['ADMIN'] } },
      { path: 'admin/bookings', component: AdminBookingsPageComponent, canActivate: [authGuard, roleGuard], data: { roles: ['ADMIN'] } }
    ]
  },
  { path: '**', redirectTo: '' }
];
