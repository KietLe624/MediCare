import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VisitPage } from './visit-page';

describe('VisitPage', () => {
  let component: VisitPage;
  let fixture: ComponentFixture<VisitPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VisitPage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VisitPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
