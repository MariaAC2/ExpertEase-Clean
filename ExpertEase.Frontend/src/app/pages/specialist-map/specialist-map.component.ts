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
    styles: this.getSimpleMapStyle()
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

  // Authentication - you can inject your auth service here
  isAuthenticated = false; // Set this based on your authentication service

  // Simple marker options for user
  userMarkerOptions: google.maps.MarkerOptions = {
    icon: {
      url: 'data:image/svg+xml;charset=UTF-8,' + encodeURIComponent(`
        <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 32 32">
          <circle cx="16" cy="16" r="14" fill="#2196F3" stroke="white" stroke-width="2"/>
          <circle cx="16" cy="16" r="6" fill="white"/>
        </svg>
      `),
      scaledSize: new google.maps.Size(32, 32),
      anchor: new google.maps.Point(16, 16)
    },
    zIndex: 1000
  };

  constructor(
    private geolocationService: GeolocationService,
    private geocodingService: GeocodingService,
    private specialistService: SpecialistService
    // Inject your auth service here
    // private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Check authentication status
    // this.isAuthenticated = this.authService.isAuthenticated();
    this.getCurrentLocation();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Simple map style - clean and minimal
  getSimpleMapStyle(): google.maps.MapTypeStyle[] {
    return [
      {
        featureType: 'poi.business',
        stylers: [{ visibility: 'off' }]
      },
      {
        featureType: 'poi.park',
        elementType: 'labels.text',
        stylers: [{ visibility: 'off' }]
      },
      {
        featureType: 'road.local',
        elementType: 'labels.icon',
        stylers: [{ visibility: 'off' }]
      }
    ];
  }

  // Simple marker options for specialists
  getMarkerOptions(marker: MapMarkerInfo): google.maps.MarkerOptions {
    const profilePictureUrl = marker.specialist.profilePictureUrl;

    if (profilePictureUrl && profilePictureUrl !== 'src/assets/avatar.svg') {
      // Use profile picture as marker
      return {
        icon: {
          url: profilePictureUrl,
          scaledSize: new google.maps.Size(40, 40),
          anchor: new google.maps.Point(20, 20)
        },
        optimized: false,
        zIndex: 100
      };
    } else {
      // Use simple colored marker
      const rating = marker.info?.rating || 0;
      let color = '#666666'; // Default gray

      if (rating >= 4.5) {
        color = '#4CAF50'; // Green for high rating
      } else if (rating >= 4.0) {
        color = '#FF9800'; // Orange for good rating
      } else if (rating >= 3.0) {
        color = '#2196F3'; // Blue for average rating
      }

      return {
        icon: {
          url: 'data:image/svg+xml;charset=UTF-8,' + encodeURIComponent(`
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 28 28">
              <circle cx="14" cy="14" r="12" fill="${color}" stroke="white" stroke-width="2"/>
              <text x="14" y="18" text-anchor="middle" fill="white" font-size="12" font-weight="bold">👤</text>
            </svg>
          `),
          scaledSize: new google.maps.Size(28, 28),
          anchor: new google.maps.Point(14, 14)
        },
        zIndex: 50
      };
    }
  }

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

  // Helper method to get the correct avatar URL
  getAvatarUrl(specialist: SpecialistDTO): string {
    return specialist.profilePictureUrl || 'assets/avatar.svg';
  }

  // Helper method to show basic tag for unauthenticated users
  shouldShowBasicTag(): boolean {
    return !this.isAuthenticated;
  }
}
