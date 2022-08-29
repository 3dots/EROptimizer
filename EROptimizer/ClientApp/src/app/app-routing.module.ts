import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AdminComponent } from './admin/admin.component'
import { ArmorDataComponent } from './armor-data/armor-data.component';
import { ArmorFiltersComponent } from './armor-filters/armor-filters.component';
import { BestClassComponent } from './best-class/best-class.component';
import { OptimizerComponent } from './optimizer/optimizer.component'

const routes: Routes = [
  { path: '', redirectTo: '/optimizer', pathMatch: 'full' },
  { path: 'admin', component: AdminComponent },
  { path: 'optimizer', component: OptimizerComponent },
  { path: 'filters', component: ArmorFiltersComponent },
  { path: 'data', component: ArmorDataComponent },
  { path: 'bestclass', component: BestClassComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
