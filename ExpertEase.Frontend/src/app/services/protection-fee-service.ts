// protection-fee.service.ts
import { Injectable } from '@angular/core';

export interface ProtectionFeeConfig {
  type: 'percentage' | 'fixed';
  percentage: number;
  fixedAmount: number;
  minFee: number;
  maxFee: number;
  enabled: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ProtectionFeeService {
  private defaultConfig: ProtectionFeeConfig = {
    type: 'percentage',
    percentage: 10, // 10%
    fixedAmount: 15, // 15 lei fixed
    minFee: 5, // Minimum 5 lei
    maxFee: 100, // Maximum 100 lei
    enabled: true
  };

  private config: ProtectionFeeConfig = { ...this.defaultConfig };

  constructor() {
    this.loadConfigFromStorage();
  }

  // Get current configuration
  getConfig(): ProtectionFeeConfig {
    return { ...this.config };
  }

  // Update configuration
  updateConfig(newConfig: Partial<ProtectionFeeConfig>): void {
    this.config = { ...this.config, ...newConfig };
    this.saveConfigToStorage();
  }

  // Calculate protection fee for a given service price
  calculateProtectionFee(servicePrice: number): number {
    if (!this.config.enabled || servicePrice <= 0) {
      return 0;
    }

    let fee: number;

    if (this.config.type === 'percentage') {
      fee = (servicePrice * this.config.percentage) / 100;
      // Apply min/max limits
      fee = Math.max(this.config.minFee, fee);
      fee = Math.min(this.config.maxFee, fee);
    } else {
      fee = this.config.fixedAmount;
    }

    return Math.round(fee * 100) / 100; // Round to 2 decimal places
  }

  // Get protection fee breakdown for display
  getProtectionFeeBreakdown(servicePrice: number): {
    fee: number;
    description: string;
    calculation: string;
  } {
    const fee = this.calculateProtectionFee(servicePrice);
    
    let description: string;
    let calculation: string;

    if (!this.config.enabled) {
      description = 'Protection fee disabled';
      calculation = '0 lei';
    } else if (this.config.type === 'percentage') {
      description = `${this.config.percentage}% protection fee`;
      calculation = `${servicePrice} × ${this.config.percentage}% = ${fee} lei`;
      
      if (fee === this.config.minFee) {
        calculation += ` (minimum ${this.config.minFee} lei applied)`;
      } else if (fee === this.config.maxFee) {
        calculation += ` (maximum ${this.config.maxFee} lei applied)`;
      }
    } else {
      description = 'Fixed protection fee';
      calculation = `${fee} lei (fixed rate)`;
    }

    return { fee, description, calculation };
  }

  // Category-specific protection fees (optional advanced feature)
  getCategorySpecificFee(servicePrice: number, categoryId?: string): number {
    // You can implement category-specific logic here
    // For example, different categories might have different fee structures
    
    const categoryOverrides: Record<string, Partial<ProtectionFeeConfig>> = {
      'plumbing': { percentage: 8, minFee: 10 },
      'electrical': { percentage: 12, minFee: 15 },
      'cleaning': { percentage: 5, minFee: 5 },
      // Add more categories as needed
    };

    if (categoryId && categoryOverrides[categoryId]) {
      const override = categoryOverrides[categoryId];
      const tempConfig = { ...this.config, ...override };
      
      if (tempConfig.type === 'percentage') {
        let fee = (servicePrice * tempConfig.percentage!) / 100;
        fee = Math.max(tempConfig.minFee!, fee);
        fee = Math.min(tempConfig.maxFee!, fee);
        return Math.round(fee * 100) / 100;
      }
    }

    return this.calculateProtectionFee(servicePrice);
  }

  // Reset to default configuration
  resetToDefault(): void {
    this.config = { ...this.defaultConfig };
    this.saveConfigToStorage();
  }

  // Enable/disable protection fee
  setEnabled(enabled: boolean): void {
    this.config.enabled = enabled;
    this.saveConfigToStorage();
  }

  // Validation methods
  isValidPercentage(percentage: number): boolean {
    return percentage >= 0 && percentage <= 50; // 0-50% range
  }

  isValidFixedAmount(amount: number): boolean {
    return amount >= 0 && amount <= 1000; // 0-1000 lei range
  }

  isValidMinMaxFee(min: number, max: number): boolean {
    return min >= 0 && max >= min && max <= 1000;
  }

  // Persistence methods
  private saveConfigToStorage(): void {
    try {
      localStorage.setItem('protectionFeeConfig', JSON.stringify(this.config));
    } catch (error) {
      console.error('Failed to save protection fee config:', error);
    }
  }

  private loadConfigFromStorage(): void {
    try {
      const stored = localStorage.getItem('protectionFeeConfig');
      if (stored) {
        const parsed = JSON.parse(stored);
        this.config = { ...this.defaultConfig, ...parsed };
      }
    } catch (error) {
      console.error('Failed to load protection fee config:', error);
      this.config = { ...this.defaultConfig };
    }
  }
}