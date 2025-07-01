import {Component, EventEmitter, Input, Output} from '@angular/core';
import {SpecialistPaginationQueryParams} from '../../models/api.models';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'app-search-filters',
  imports: [
    FormsModule
  ],
  templateUrl: './search-filters.component.html',
  styleUrl: './search-filters.component.scss'
})
export class SearchFiltersComponent {
  @Input() initialFilters: Partial<SpecialistPaginationQueryParams> = {};
  @Output() filtersChange = new EventEmitter<Partial<SpecialistPaginationQueryParams>>();

  filters: Partial<SpecialistPaginationQueryParams> = {
    search: '',
    categoryName: '',
    minRating: undefined,
    maxRating: undefined,
    experienceRange: undefined,
    sortByRating: undefined
  };

  ngOnInit() {
    // Initialize filters with input values
    this.filters = { ...this.filters, ...this.initialFilters };
  }

  onFiltersChange() {
    // Clean up empty string values
    const cleanedFilters: Partial<SpecialistPaginationQueryParams> = {};

    Object.keys(this.filters).forEach(key => {
      const value = (this.filters as any)[key];
      if (value !== '' && value !== null && value !== undefined) {
        (cleanedFilters as any)[key] = value;
      }
    });

    this.filtersChange.emit(cleanedFilters);
  }

  clearAllFilters() {
    this.filters = {
      search: '',
      categoryName: '',
      minRating: undefined,
      maxRating: undefined,
      experienceRange: undefined,
      sortByRating: undefined
    };
    this.onFiltersChange();
  }

  applyQuickFilter(type: 'topRated' | 'experienced' | 'highRated') {
    switch (type) {
      case 'topRated':
        this.filters.sortByRating = 'desc';
        break;
      case 'experienced':
        this.filters.experienceRange = '7-10';
        break;
      case 'highRated':
        this.filters.minRating = 4.5;
        this.filters.sortByRating = 'desc';
        break;
    }
    this.onFiltersChange();
  }
}
