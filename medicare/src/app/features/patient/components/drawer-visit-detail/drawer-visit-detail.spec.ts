import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrawerVisitDetailComponent } from './drawer-visit-detail';

describe('DrawerVisitDetail', () => {
  let component: DrawerVisitDetailComponent;
  let fixture: ComponentFixture<DrawerVisitDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DrawerVisitDetailComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(DrawerVisitDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
