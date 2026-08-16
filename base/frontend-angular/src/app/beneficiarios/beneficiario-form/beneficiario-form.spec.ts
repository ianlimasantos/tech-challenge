import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BeneficiarioForm } from './beneficiario-form';

describe('BeneficiarioForm', () => {
  let component: BeneficiarioForm;
  let fixture: ComponentFixture<BeneficiarioForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BeneficiarioForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BeneficiarioForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
