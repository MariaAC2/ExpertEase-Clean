import {Component, EventEmitter, Input, Output} from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableColumn } from '../../models/table.models';
import {FormsModule} from '@angular/forms'; // adjust path

@Component({
  selector: 'app-dynamic-table',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dynamic-table.component.html',
  styleUrls: ['./dynamic-table.component.scss']
})
export class DynamicTableComponent {
  @Input() columns: TableColumn[] = [];
  @Input() data: any[] = [];

  /**
   * Get the primary column (first non-action column) for mobile card titles
   */
  getPrimaryColumn(): TableColumn | undefined {
    return this.columns.find(col => col.type !== 'action');
  }

  /**
   * Get the action column for mobile card actions
   */
  getActionColumn(): TableColumn | undefined {
    return this.columns.find(col => col.type === 'action');
  }

  /**
   * Get all non-action columns for mobile card body
   */
  getNonActionColumns(): TableColumn[] {
    return this.columns.filter(col => col.type !== 'action');
  }

  /**
   * Check if column is action type
   */
  isActionColumn(column: TableColumn): boolean {
    return column.type === 'action';
  }

  /**
   * Get the display value for a column and row
   */
  getDisplayValue(column: TableColumn, row: any): string {
    if (column.compute) {
      return column.compute(row);
    }
    return row[column.key] || '';
  }

  /**
   * Truncate text for better mobile display
   */
  truncateText(text: string, maxLength: number = 50): string {
    if (!text) return '';
    return text.length > maxLength ? text.substring(0, maxLength) + '...' : text;
  }

  /**
   * Get max text length based on screen size
   */
  getMaxLength(): number {
    if (typeof window === 'undefined') return 50;

    const width = window.innerWidth;
    if (width <= 480) return 15;
    if (width <= 768) return 25;
    if (width <= 1024) return 35;
    return 50;
  }

  /**
   * Check if we're on mobile device
   */
  isMobile(): boolean {
    if (typeof window === 'undefined') return false;
    return window.innerWidth <= 768;
  }

  /**
   * Determine if a column should be hidden based on priority and screen size
   */
  shouldHideColumn(column: TableColumn): boolean {
    if (typeof window === 'undefined') return false;

    const width = window.innerWidth;
    const columnIndex = this.columns.indexOf(column);

    // Never hide action columns
    if (this.isActionColumn(column)) return false;

    // Hide columns based on screen size and priority
    if (width <= 480) {
      // On very small screens, show only first 2 columns + actions
      return columnIndex >= 2 && !this.isActionColumn(column);
    } else if (width <= 600) {
      // On small screens, show only first 3 columns + actions
      return columnIndex >= 3 && !this.isActionColumn(column);
    } else if (width <= 768) {
      // On tablets, show only first 4 columns + actions
      return columnIndex >= 4 && !this.isActionColumn(column);
    } else if (width <= 900) {
      // On small desktops, hide less important columns
      return this.getColumnPriority(column) >= 3;
    }

    return false;
  }

  /**
   * Get priority order for columns (for responsive hiding)
   */
  getColumnPriority(column: TableColumn): number {
    // Action columns always have highest priority
    if (this.isActionColumn(column)) return 0;

    // Primary identifier columns (like name, id) have high priority
    if (column.key?.toLowerCase().includes('name') ||
      column.key?.toLowerCase().includes('id') ||
      column.key?.toLowerCase().includes('title')) {
      return 1;
    }

    // Status and important fields
    if (column.key?.toLowerCase().includes('status') ||
      column.key?.toLowerCase().includes('date') ||
      column.key?.toLowerCase().includes('email')) {
      return 2;
    }

    // Everything else has lower priority
    return 3;
  }

  /**
   * Get short action labels for mobile
   */
  getShortActionLabel(label: string): string {
    const shortLabels: { [key: string]: string } = {
      'Detalii': 'Det',
      'Modifică': 'Mod',
      'Șterge': 'Șt',
      'View': 'V',
      'Edit': 'E',
      'Delete': 'D',
      'Details': 'Det'
    };

    return shortLabels[label] || label.substring(0, 3);
  }

  /**
   * Sort columns by priority for responsive display
   */
  getSortedColumnsByPriority(): TableColumn[] {
    return [...this.columns].sort((a, b) =>
      this.getColumnPriority(a) - this.getColumnPriority(b)
    );
  }

  /**
   * Check if a column should be hidden on current screen size
   */
  isColumnHidden(columnIndex: number): boolean {
    if (typeof window === 'undefined') return false;

    const screenWidth = window.innerWidth;

    if (screenWidth <= 480) {
      return columnIndex >= 3; // Hide after 2 regular columns + 1 action column
    } else if (screenWidth <= 600) {
      return columnIndex >= 4; // Hide after 3 regular columns + 1 action column
    } else if (screenWidth <= 768) {
      return columnIndex >= 5; // Hide after 4 regular columns + 1 action column
    } else if (screenWidth <= 900) {
      return columnIndex >= 6; // Hide after 5 regular columns + 1 action column
    }

    return false; // Show all columns on large screens
  }
}
