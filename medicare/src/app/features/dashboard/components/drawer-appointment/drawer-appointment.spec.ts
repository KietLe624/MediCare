import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrawerAppointment } from './drawer-appointment';

describe('DrawerAppointment', () => {
  let component: DrawerAppointment;
  let fixture: ComponentFixture<DrawerAppointment>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrawerAppointment]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DrawerAppointment);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
