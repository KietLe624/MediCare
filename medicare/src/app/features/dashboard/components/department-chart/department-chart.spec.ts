import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DepartmentChart } from './department-chart';

describe('DepartmentChart', () => {
  let component: DepartmentChart;
  let fixture: ComponentFixture<DepartmentChart>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DepartmentChart]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DepartmentChart);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
