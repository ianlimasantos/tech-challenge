import { Routes } from '@angular/router';
import { BeneficiarioForm } from './beneficiarios/beneficiario-form/beneficiario-form';
import { PlanosLista } from './planos/planos-lista';
import { BeneficiarioLista } from './beneficiarios/beneficiario-lista/beneficiario-lista';

export const routes: Routes = [
  {
    path: 'planos',
    component: PlanosLista
  },
  {
    path: 'beneficiario',
    component: BeneficiarioForm
  },
  {
    path: 'beneficiarios/:id/editar',
    component: BeneficiarioForm
  },
  {
    path: 'beneficiario-lista',
    component: BeneficiarioLista
  },
];
