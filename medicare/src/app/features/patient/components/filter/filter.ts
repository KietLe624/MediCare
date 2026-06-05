import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from "@angular/common";
import { debounceTime, distinctUntilChanged, Subject, Subscription } from 'rxjs';
import { ClickOutside } from "../../../../core/shared/directives/click-outside";

// models
import { PatientQueryParams } from "../../models/patient.model";

@Component({
  selector: 'app-filter',
  imports: [ClickOutside, CommonModule],
  templateUrl: './filter.html',
  styleUrl: './filter.scss',
})
export class FilterComponent implements OnInit {

  @Output()
  filterChanged = new EventEmitter<PatientQueryParams>();

  filters: PatientQueryParams = {
    search: '',
    patientType: '',
    bloodType: '',
    sortBy: 'asc',
  };

  onChange() {
    this.filterChanged.emit(this.filters);
  }
  isFilterOpen = false;

  applyFilters() {
    this.isFilterOpen = false; // Đóng menu
    this.filterChanged.emit(this.filters); // Phát sự kiện với bộ lọc hiện tại
    console.log('Bộ lọc đang áp dụng:', this.filters);
  }

  resetFilters() {
    // Reset lại toàn bộ về rỗng
    this.filters = {
      search: this.filters.search, // Giữ lại từ khóa tìm kiếm (nếu muốn)
      patientType: '',
      bloodType: '',
      sortBy: 'asc',
    };
    this.applyFilters(); // Áp dụng bộ lọc đã reset
  }


  private searchSubject = new Subject<string>();
  private searchSubscription?: Subscription;


  ngOnInit() {
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(500), // Đợi 300ms sau khi người dùng ngừng gõ
      distinctUntilChanged() // Chỉ phát khi giá trị thay đổi
    ).subscribe(searchTerm => {
      this.filters.search = searchTerm;
      this.applyFilters();
    });
  }
  ngOnDestroy() {
    this.searchSubscription?.unsubscribe();
  }

  onSearchInput(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.searchSubject.next(value);
  }

}
