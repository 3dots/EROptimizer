import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AdminComponent } from './admin/admin.component'
import { OptimizerComponent } from './optimizer/optimizer.component'

const routes: Routes = [
  { path: '', redirectTo: '/optimizer', pathMatch: 'full' },
  { path: 'optimizer', component: OptimizerComponent },
  { path: 'admin', component: AdminComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
