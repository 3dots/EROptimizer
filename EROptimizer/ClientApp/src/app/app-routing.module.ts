import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AdminComponent } from './admin/admin.component'
import { ArmorPiecesComponent } from './armor-pieces/armor-pieces.component';
import { ArmorSelectionsComponent } from './armor-selections/armor-selections.component';
import { OptimizerComponent } from './optimizer/optimizer.component'

const routes: Routes = [
  { path: '', redirectTo: '/optimizer', pathMatch: 'full' },
  { path: 'admin', component: AdminComponent },
  { path: 'optimizer', component: OptimizerComponent },
  { path: 'armor', component: ArmorSelectionsComponent },
  { path: 'pieces', component: ArmorPiecesComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
