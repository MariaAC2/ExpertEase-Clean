import {Component, OnDestroy, OnInit} from '@angular/core';
import {Subject, takeUntil} from 'rxjs';
import {MapMarkerInfo, SpecialistDTO} from '../../models/api.models';
import {GeolocationService} from '../../services/geolocation.service';
import {GeocodingService} from '../../services/geocoding.service';
import {SpecialistService} from '../../services/specialist.service';
import {NgForOf, NgIf} from '@angular/common';
import {GoogleMap, MapInfoWindow, MapMarker} from '@angular/google-maps';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-specialist-map',
  imports: [
    NgIf,
    GoogleMap,
    NgForOf,
    MapInfoWindow,
    RouterLink,
    MapMarker
  ],
  templateUrl: './specialist-map.component.html',
  styleUrl: './specialist-map.component.scss'
})
export class SpecialistMapComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Map properties
  mapCenter: google.maps.LatLngLiteral = { lat: 44.4268, lng: 26.1025 };
  mapZoom = 13;
  mapTypeId: string = 'roadmap';
  mapOptions: google.maps.MapOptions = {
    mapTypeControl: false,
    streetViewControl: false,
    fullscreenControl: false,
    zoomControl: true,
    gestureHandling: 'greedy',
    styles: this.getCustomMapStyle()
  };

  // Info window options
  infoWindowOptions: google.maps.InfoWindowOptions = {
    disableAutoPan: false,
    maxWidth: 350,
    pixelOffset: new google.maps.Size(0, -10)
  };

  // Location properties
  userLocation: google.maps.LatLngLiteral | null = null;
  isLoadingLocation = false;
  locationError: string | null = null;

  // Specialists properties
  specialists: SpecialistDTO[] = [];
  specialistMarkers: MapMarkerInfo[] = [];
  isLoadingSpecialists = false;
  selectedMarker: MapMarkerInfo | null = null;

  // UI properties
  sidePanelOpen = false;
  averageRating = 0;

  // Enhanced marker options
  userMarkerOptions: google.maps.MarkerOptions = {
    icon: {
      url: 'data:image/svg+xml;charset=UTF-8,' + encodeURIComponent(`
        <svg xmlns="http://www.w3.org/2000/svg" width="50" height="50" viewBox="0 0 50 50">
          <defs>
            <radialGradient id="userGrad" cx="50%" cy="50%" r="50%">
              <stop offset="0%" style="stop-color:#6c5ce7;stop-opacity:1" />
              <stop offset="100%" style="stop-color:#3c1a7d;stop-opacity:1" />
            </radialGradient>
          </defs>
          <circle cx="25" cy="25" r="23" fill="url(#userGrad)" stroke="white" stroke-width="3"/>
          <circle cx="25" cy="25" r="12" fill="white"/>
          <text x="25" y="30" text-anchor="middle" fill="#3c1a7d" font-size="16" font-weight="bold">📍</text>
        </svg>
      `),
      scaledSize: new google.maps.Size(50, 50),
      anchor: new google.maps.Point(25, 25)
    },
    zIndex: 1000
  };

  constructor(
    private geolocationService: GeolocationService,
    private geocodingService: GeocodingService,
    private specialistService: SpecialistService
  ) {}

  ngOnInit(): void {
    this.getCurrentLocation();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Get custom map style for better appearance
  getCustomMapStyle(): google.maps.MapTypeStyle[] {
    return [
      {
        featureType: 'poi',
        elementType: 'labels',
        stylers: [{ visibility: 'off' }]
      },
      {
        featureType: 'transit',
        elementType: 'labels',
        stylers: [{ visibility: 'off' }]
      },
      {
        featureType: 'road',
        elementType: 'labels.icon',
        stylers: [{ visibility: 'off' }]
      },
      {
        featureType: 'water',
        elementType: 'geometry',
        stylers: [{ color: '#a2daf2' }]
      },
      {
        featureType: 'landscape',
        elementType: 'geometry',
        stylers: [{ color: '#f5f5f5' }]
      }
    ];
  }

  // Get enhanced marker options for specialists
  getMarkerOptions(marker: MapMarkerInfo): google.maps.MarkerOptions {
    const rating = marker.info?.rating || 0;
    const color = rating >= 4.5 ? '#28a745' : rating >= 4 ? '#ffc107' : '#6c757d';

    return {
      icon: {
        url: 'data:image/svg+xml;charset=UTF-8,' + encodeURIComponent(`
          <svg xmlns="http://www.w3.org/2000/svg" width="40" height="40" viewBox="0 0 40 40">
            <defs>
              <radialGradient id="grad${marker.specialist.id}" cx="50%" cy="50%" r="50%">
                <stop offset="0%" style="stop-color:${color};stop-opacity:0.8" />
                <stop offset="100%" style="stop-color:${color};stop-opacity:1" />
              </radialGradient>
            </defs>
            <circle cx="20" cy="20" r="18" fill="url(#grad${marker.specialist.id})" stroke="white" stroke-width="2"/>
            <text x="20" y="25" text-anchor="middle" fill="white" font-size="20" font-weight="bold">👨‍💼</text>
          </svg>
        `),
        scaledSize: new google.maps.Size(40, 40),
        anchor: new google.maps.Point(20, 20)
      },
      animation: google.maps.Animation.DROP,
      zIndex: rating >= 4.5 ? 100 : 50
    };
  }

  // Rest of your existing methods...
  getCurrentLocation(): void {
    this.isLoadingLocation = true;
    this.locationError = null;

    this.geolocationService.getCurrentPosition()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (position) => {
          this.userLocation = {
            lat: position.coords.latitude,
            lng: position.coords.longitude
          };
          this.mapCenter = this.userLocation;
          this.isLoadingLocation = false;
          this.loadSpecialists();
        },
        error: (error) => {
          this.isLoadingLocation = false;
          this.handleLocationError(error);
          this.useDefaultLocation();
        }
      });
  }

  handleLocationError(error: any): void {
    switch (error.code) {
      case 1:
        this.locationError = 'Accesul la locație a fost refuzat. Te rugăm să acorzi permisiunea.';
        break;
      case 2:
        this.locationError = 'Locația nu poate fi determinată.';
        break;
      case 3:
        this.locationError = 'Timpul pentru obținerea locației a expirat.';
        break;
      default:
        this.locationError = 'Nu s-a putut obține locația curentă.';
    }
  }

  useDefaultLocation(): void {
    this.mapCenter = { lat: 44.4268, lng: 26.1025 };
    this.locationError = null;
    this.loadSpecialists();
  }

  loadSpecialists(): void {
    this.isLoadingSpecialists = true;

    this.specialistService.getSpecialists('', 1, 100)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.specialists = response.response?.data || [];
          this.createSpecialistMarkers();
          this.calculateAverageRating();
          this.isLoadingSpecialists = false;
        },
        error: (error) => {
          console.error('Error loading specialists:', error);
          this.isLoadingSpecialists = false;
        }
      });
  }

  async createSpecialistMarkers(): Promise<void> {
    this.specialistMarkers = [];

    for (const specialist of this.specialists) {
      if (specialist.address) {
        try {
          const position = await this.geocodingService.geocodeAddress(specialist.address);
          if (position) {
            const distance = this.userLocation ?
              this.geolocationService.calculateDistance(
                this.userLocation.lat,
                this.userLocation.lng,
                position.lat,
                position.lng
              ) : undefined;

            const marker: MapMarkerInfo = {
              position,
              title: specialist.fullName,
              specialist,
              info: {
                name: specialist.fullName,
                description: specialist.description || 'Specialist pe platforma ExpertEase',
                rating: specialist.rating || 0,
                address: specialist.address,
                distance: distance ? Math.round(distance * 100) / 100 : undefined,
              }
            };

            this.specialistMarkers.push(marker);
          }
        } catch (error) {
          console.error(`Failed to geocode address for ${specialist.fullName}:`, error);
        }
      }
    }

    if (this.userLocation) {
      this.specialistMarkers.sort((a, b) => (a.info?.distance || 999) - (b.info?.distance || 999));
    }
  }

  calculateAverageRating(): void {
    if (this.specialists.length > 0) {
      const total = this.specialists.reduce((sum, spec) => sum + (spec.rating || 0), 0);
      this.averageRating = Math.round((total / this.specialists.length) * 10) / 10;
    }
  }

  onMarkerClick(marker: MapMarkerInfo): void {
    this.selectedMarker = marker;
    this.mapCenter = marker.position;
    this.mapZoom = 15;
  }

  closeInfoWindow(): void {
    this.selectedMarker = null;
  }

  centerOnUser(): void {
    if (this.userLocation) {
      this.mapCenter = this.userLocation;
      this.mapZoom = 15;
    }
  }

  toggleSatelliteView(): void {
    this.mapTypeId = this.mapTypeId === 'satellite' ? 'roadmap' : 'satellite';
    this.mapOptions = {
      ...this.mapOptions,
      mapTypeId: this.mapTypeId as any
    };
  }

  toggleSidePanel(): void {
    this.sidePanelOpen = !this.sidePanelOpen;
  }

  selectSpecialistFromList(marker: MapMarkerInfo): void {
    this.onMarkerClick(marker);
    this.sidePanelOpen = false;
  }

  getSortedSpecialists(): MapMarkerInfo[] {
    return [...this.specialistMarkers].sort((a, b) =>
      (a.info?.distance || 999) - (b.info?.distance || 999)
    );
  }

  getNearbyCount(): number {
    return this.specialistMarkers.filter(marker =>
      (marker.info?.distance || 999) <= 10
    ).length;
  }

  getCategoriesText(categories: string[]): string {
    return categories.slice(0, 2).join(', ') + (categories.length > 2 ? '...' : '');
  }

  getStars(rating: number): string {
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 >= 0.5;
    let stars = '⭐'.repeat(fullStars);
    if (hasHalfStar) stars += '⭐';
    return stars || '☆☆☆☆☆';
  }

  trackBySpecialist(index: number, marker: MapMarkerInfo): string {
    return marker.specialist.id;
  }
}
