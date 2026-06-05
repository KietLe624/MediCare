import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreatePatient } from './create-patient';

describe('CreatePatient', () => {
  let component: CreatePatient;
  let fixture: ComponentFixture<CreatePatient>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreatePatient]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreatePatient);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
