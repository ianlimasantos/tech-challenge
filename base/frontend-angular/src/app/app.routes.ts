import { Routes } from '@angular/router';
import { BeneficiarioForm } from './beneficiarios/beneficiario-form/beneficiario-form';
import { PlanosLista } from './planos/planos-lista';
import { BeneficiarioLista } from './beneficiarios/beneficiario-lista/beneficiario-lista';
import { BeneficiarioEspecifico } from './beneficiarios/beneficiario-especifico/beneficiario-especifico';

export const routes: Routes = [
  {
    path: '',
    component: PlanosLista
  },
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
  {
    path: 'beneficiario-especifico',
    component: BeneficiarioEspecifico
  },
];
