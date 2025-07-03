import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SpecialistFilterParams, CategoryDTO } from '../../models/api.models';
import { CategoryService } from '../../services/category.service';

type FilterType = 'categories' | 'rating' | 'experience' | 'sort' | null;

@Component({
  selector: 'app-search-filters',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './search-filters.component.html',
  styleUrls: ['./search-filters.component.scss']
})
export class SearchFiltersComponent implements OnInit {
  @Input() initialFilters: SpecialistFilterParams = {};
  @Output() filtersChange = new EventEmitter<SpecialistFilterParams>();

  isOpen = false;
  selectedFilterType: FilterType = null;
  currentFilters: SpecialistFilterParams = {};

  // Categories
  allCategories: CategoryDTO[] = [];
  filteredCategories: CategoryDTO[] = [];
  categorySearchTerm = '';
  loadingCategories = false;

  // Rating options with stars
  ratingOptions = [
    { value: 5, stars: '⭐⭐⭐⭐⭐', label: '5 stele' },
    { value: 4, stars: '⭐⭐⭐⭐', label: '4+ stele' },
    { value: 3, stars: '⭐⭐⭐', label: '3+ stele' }
  ];

  experienceOptions = [
    { value: '0-2', label: '0-2 ani' },
    { value: '2-5', label: '2-5 ani' },
    { value: '5-7', label: '5-7 ani' },
    { value: '7-10', label: '7-10 ani' },
    { value: '10+', label: '10+ ani' }
  ];

  constructor(private categoryService: CategoryService) {}

  ngOnInit() {
    this.currentFilters = { ...this.initialFilters };
    this.loadCategories();
  }

  get activeFiltersCount(): number {
    let count = 0;
    if (this.currentFilters.categoryIds && this.currentFilters.categoryIds.length > 0) count++;
    if (this.currentFilters.minRating) count++;
    if (this.currentFilters.experienceRange) count++;
    if (this.currentFilters.sortByRating) count++;
    return count;
  }

  get selectedCategoriesCount(): number {
    return this.currentFilters.categoryIds ? this.currentFilters.categoryIds.length : 0;
  }

  loadCategories() {
    this.loadingCategories = true;
    this.categoryService.getAllCategories().subscribe({
      next: (response) => {
        this.allCategories = response.response || [];
        this.filteredCategories = [...this.allCategories];
        this.loadingCategories = false;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.loadingCategories = false;
      }
    });
  }

  searchCategories() {
    if (!this.categorySearchTerm.trim()) {
      this.filteredCategories = [...this.allCategories];
    } else {
      const searchTerm = this.categorySearchTerm.toLowerCase();
      this.filteredCategories = this.allCategories.filter(cat =>
        cat.name.toLowerCase().includes(searchTerm) ||
        (cat.description && cat.description.toLowerCase().includes(searchTerm))
      );
    }
  }

  toggleFilterPanel() {
    this.isOpen = !this.isOpen;
    if (!this.isOpen) {
      this.selectedFilterType = null;
    }
  }

  selectFilterType(type: FilterType) {
    this.selectedFilterType = type;
  }

  goBack() {
    this.selectedFilterType = null;
  }

  // Category methods
  toggleCategory(categoryId: string) {
    if (!this.currentFilters.categoryIds) {
      this.currentFilters.categoryIds = [];
    }

    const index = this.currentFilters.categoryIds.indexOf(categoryId);
    if (index > -1) {
      this.currentFilters.categoryIds.splice(index, 1);
    } else {
      this.currentFilters.categoryIds.push(categoryId);
    }

    this.onFiltersChange();
  }

  isCategorySelected(categoryId: string): boolean {
    return this.currentFilters.categoryIds ? this.currentFilters.categoryIds.includes(categoryId) : false;
  }

  clearCategoriesFilter() {
    this.currentFilters.categoryIds = [];
    this.onFiltersChange();
  }

  // Rating methods
  selectRating(value: number) {
    this.currentFilters.minRating = value;
    this.onFiltersChange();
  }

  clearRatingFilter() {
    this.currentFilters.minRating = undefined;
    this.onFiltersChange();
  }

  // Experience methods
  selectExperience(value: string) {
    this.currentFilters.experienceRange = value;
    this.onFiltersChange();
  }

  clearExperienceFilter() {
    this.currentFilters.experienceRange = undefined;
    this.onFiltersChange();
  }

  // Sort methods
  selectSort(value: 'asc' | 'desc') {
    this.currentFilters.sortByRating = value;
    this.onFiltersChange();
  }

  clearSortFilter() {
    this.currentFilters.sortByRating = undefined;
    this.onFiltersChange();
  }

  clearAllFilters() {
    this.currentFilters = {};
    this.onFiltersChange();
    this.isOpen = false;
    this.selectedFilterType = null;
  }

  onFiltersChange() {
    // Clean up empty values
    const cleanedFilters: SpecialistFilterParams = {};

    if (this.currentFilters.categoryIds && this.currentFilters.categoryIds.length > 0) {
      cleanedFilters.categoryIds = this.currentFilters.categoryIds;
    }

    if (this.currentFilters.minRating) {
      cleanedFilters.minRating = this.currentFilters.minRating;
    }

    if (this.currentFilters.experienceRange) {
      cleanedFilters.experienceRange = this.currentFilters.experienceRange;
    }

    if (this.currentFilters.sortByRating) {
      cleanedFilters.sortByRating = this.currentFilters.sortByRating;
    }

    this.filtersChange.emit(cleanedFilters);
  }

  getRatingLabel(rating: number): string {
    const option = this.ratingOptions.find(opt => opt.value === rating);
    return option ? option.label : `${rating}+ stele`;
  }

  getExperienceLabel(value: string): string {
    const option = this.experienceOptions.find(opt => opt.value === value);
    return option ? option.label : value;
  }

  getSortLabel(value: string): string {
    return value === 'desc' ? 'Descrescător' : 'Crescător';
  }
}
