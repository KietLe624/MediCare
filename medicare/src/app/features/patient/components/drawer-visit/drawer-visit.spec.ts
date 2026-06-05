import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrawerVisit } from './drawer-visit';

describe('DrawerVisit', () => {
  let component: DrawerVisit;
  let fixture: ComponentFixture<DrawerVisit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrawerVisit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DrawerVisit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
