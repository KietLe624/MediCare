import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditPatient } from './edit-patient';

describe('EditPatient', () => {
  let component: EditPatient;
  let fixture: ComponentFixture<EditPatient>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditPatient]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditPatient);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
