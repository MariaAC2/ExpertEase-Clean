import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {NgForOf, NgIf} from '@angular/common';
import {CategoryDTO} from '../../models/api.models';
import {ProfileService} from '../../services/profile.service';
import {SearchInputComponent} from '../search-input/search-input.component';

@Component({
  selector: 'app-category-selector',
  imports: [
    FormsModule,
    NgForOf,
    NgIf,
    SearchInputComponent
  ],
  templateUrl: './category-selector.component.html',
  styleUrl: './category-selector.component.scss'
})
export class CategorySelectorComponent implements OnInit {
  category: string | null = null;
  allCategories: CategoryDTO[] | undefined = [];
  searchTerm: string = '';

  dummyCategories: CategoryDTO[] = [
    {
      id: 'cat-001',
      name: 'Traduceri medicale',
      description: 'Traducerea documentelor medicale de specialitate.'
    },
    {
      id: 'cat-002',
      name: 'Logopedie',
      description: 'Servicii de diagnostic și terapie pentru tulburări de vorbire.'
    },
    {
      id: 'cat-003',
      name: 'Psihoterapie',
      description: 'Sesiuni individuale sau de grup pentru sănătatea mintală.'
    },
    {
      id: 'cat-004',
      name: 'Nutriție clinică',
      description: 'Planuri alimentare personalizate pentru afecțiuni cronice.'
    },
    {
      id: 'cat-005',
      name: 'Fizioterapie',
      description: 'Reabilitare fizică post-traumatică sau post-operatorie.'
    }
  ];

  constructor(public categoryService: ProfileService) {}

  @Input() selectedCategoryIds: string[] = [];
  @Output() categoryChange = new EventEmitter<string[]>();

  ngOnInit() {
    this.allCategories = this.dummyCategories;
    // this.getCategories();
  }

  getCategories() {
    this.categoryService.getCategories(this.searchTerm).subscribe(response => {
      this.allCategories = response.response;
    });
  }

  toggle(id: string) {
    if (this.selectedCategoryIds.includes(id)) return;
    this.selectedCategoryIds.push(id);
    this.categoryChange.emit(this.selectedCategoryIds);
  }

  add() {
    if (this.category && !this.selectedCategoryIds.includes(this.category)) {
      this.selectedCategoryIds.push(this.category);
      this.categoryChange.emit(this.selectedCategoryIds);
      this.category = null;
    }
  }

  remove(id: string) {
    this.selectedCategoryIds = this.selectedCategoryIds.filter(c => c !== id);
    this.categoryChange.emit(this.selectedCategoryIds);
  }

  getCategoryName(id: string): string {
    return this.allCategories?.find(c => c.id === id)?.name || 'Categorie necunoscută';
  }

  onSearch(term: string) {
    this.searchTerm = term;
    // this.getCategories();
  }
}
