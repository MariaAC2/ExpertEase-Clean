import {Component, OnDestroy, OnInit, ViewChild} from '@angular/core';
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

  // ViewChild to access the info window
  @ViewChild('infoWindow') infoWindow!: MapInfoWindow;

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
        <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 48 48">
          <circle cx="24" cy="24" r="20" fill="#2196F3" stroke="white" stroke-width="3"/>
          <circle cx="24" cy="24" r="8" fill="white"/>
        </svg>
      `),
      scaledSize: new google.maps.Size(48, 48),
      anchor: new google.maps.Point(24, 24)
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
    // Ensure marker and specialist data exists
    if (!marker || !marker.specialist) {
      return this.getDefaultMarkerOptions();
    }

    const profilePictureUrl = marker.specialist.profilePictureUrl;

    // Check if we have a valid profile picture URL
    if (profilePictureUrl &&
      profilePictureUrl !== 'src/assets/avatar.svg' &&
      profilePictureUrl.startsWith('http')) {

      return {
        icon: {
          url: profilePictureUrl,
          scaledSize: new google.maps.Size(40, 40),
          anchor: new google.maps.Point(20, 20)
        },
        optimized: false,
        zIndex: 100,
        title: marker.title || marker.specialist.fullName || 'Specialist'
      };
    } else {
      return this.getColorCodedMarkerOptions(marker);
    }
  }

  private getColorCodedMarkerOptions(marker: MapMarkerInfo): google.maps.MarkerOptions {
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
          <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 32 32">
            <circle cx="16" cy="16" r="14" fill="${color}" stroke="white" stroke-width="2"/>
            <text x="16" y="20" text-anchor="middle" fill="white" font-size="14" font-weight="bold">👤</text>
          </svg>
        `),
        scaledSize: new google.maps.Size(32, 32),
        anchor: new google.maps.Point(16, 16)
      },
      zIndex: 50,
      title: marker.title || marker.specialist?.fullName || 'Specialist'
    };
  }

  private getDefaultMarkerOptions(): google.maps.MarkerOptions {
    return {
      icon: {
        url: 'data:image/svg+xml;charset=UTF-8,' + encodeURIComponent(`
          <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 32 32">
            <circle cx="16" cy="16" r="14" fill="#666666" stroke="white" stroke-width="2"/>
            <text x="16" y="20" text-anchor="middle" fill="white" font-size="14" font-weight="bold">?</text>
          </svg>
        `),
        scaledSize: new google.maps.Size(32, 32),
        anchor: new google.maps.Point(16, 16)
      },
      zIndex: 10
    };
  }

  getCurrentLocation(): void {
    this.isLoadingLocation = true;
    this.locationError = null;

    // Check if geolocation is supported
    if (!navigator.geolocation) {
      this.isLoadingLocation = false;
      this.locationError = 'Geolocația nu este suportată de acest browser.';
      this.useDefaultLocation();
      return;
    }

    // Set a timeout to prevent indefinite loading
    const timeoutId = setTimeout(() => {
      this.isLoadingLocation = false;
      this.locationError = 'Timpul pentru obținerea locației a expirat.';
      this.useDefaultLocation();
    }, 10000); // 10 second timeout

    this.geolocationService.getCurrentPosition()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (position) => {
          clearTimeout(timeoutId);

          // Validate the position
          if (this.isValidPosition({
            lat: position.coords.latitude,
            lng: position.coords.longitude
          })) {
            this.userLocation = {
              lat: position.coords.latitude,
              lng: position.coords.longitude
            };
            this.mapCenter = this.userLocation;
            this.isLoadingLocation = false;
            this.locationError = null;

            // Load specialists after getting user location
            this.loadSpecialists();
          } else {
            this.isLoadingLocation = false;
            this.handleLocationError({ code: 2, message: 'Invalid coordinates received' });
            this.useDefaultLocation();
          }
        },
        error: (error) => {
          clearTimeout(timeoutId);
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
    // Clear existing markers to prevent glitches
    this.specialistMarkers = [];
    this.selectedMarker = null;

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
          this.specialists = [];
          this.specialistMarkers = [];
          this.isLoadingSpecialists = false;
        }
      });
  }

  async createSpecialistMarkers(): Promise<void> {
    // Clear existing markers first
    this.specialistMarkers = [];

    // Process specialists in batches to avoid overwhelming the geocoding service
    const batchSize = 5;
    const batches = [];

    for (let i = 0; i < this.specialists.length; i += batchSize) {
      batches.push(this.specialists.slice(i, i + batchSize));
    }

    for (const batch of batches) {
      await Promise.all(batch.map(async (specialist) => {
        if (specialist.address && specialist.address.trim()) {
          try {
            // Add delay to prevent rate limiting
            await new Promise(resolve => setTimeout(resolve, 100));

            const position = await this.geocodingService.geocodeAddress(specialist.address);

            if (position && this.isValidPosition(position)) {
              const distance = this.userLocation ?
                this.geolocationService.calculateDistance(
                  this.userLocation.lat,
                  this.userLocation.lng,
                  position.lat,
                  position.lng
                ) : undefined;

              const marker: MapMarkerInfo = {
                position,
                title: specialist.fullName || 'Specialist',
                specialist,
                info: {
                  name: specialist.fullName || 'Specialist',
                  description: specialist.description || 'Specialist pe platforma ExpertEase',
                  rating: specialist.rating || 0,
                  address: specialist.address,
                  distance: distance ? Math.round(distance * 100) / 100 : undefined,
                }
              };

              // Add marker safely
              this.specialistMarkers = [...this.specialistMarkers, marker];
            }
          } catch (error) {
            console.warn(`Failed to geocode address for ${specialist.fullName}: ${specialist.address}`, error);
          }
        }
      }));
    }

    // Sort by distance if user location is available
    if (this.userLocation && this.specialistMarkers.length > 0) {
      this.specialistMarkers.sort((a, b) => (a.info?.distance || 999) - (b.info?.distance || 999));
    }

    console.log(`Successfully created ${this.specialistMarkers.length} markers from ${this.specialists.length} specialists`);
  }

  // Helper method to validate coordinates
  private isValidPosition(position: google.maps.LatLngLiteral): boolean {
    return position.lat >= -90 && position.lat <= 90 &&
      position.lng >= -180 && position.lng <= 180 &&
      !isNaN(position.lat) && !isNaN(position.lng);
  }

  calculateAverageRating(): void {
    if (this.specialists.length > 0) {
      const total = this.specialists.reduce((sum, spec) => sum + (spec.rating || 0), 0);
      this.averageRating = Math.round((total / this.specialists.length) * 10) / 10;
    }
  }

  onMarkerClick(marker: MapMarkerInfo): void {
    // Validate marker data before setting
    if (marker && marker.position && marker.specialist) {
      this.selectedMarker = marker;

      // Safely update map center and zoom
      if (this.isValidPosition(marker.position)) {
        this.mapCenter = { ...marker.position }; // Create new object to trigger change detection
        this.mapZoom = Math.max(15, this.mapZoom); // Ensure minimum zoom level
      }
    }
  }

  closeInfoWindow(): void {
    this.selectedMarker = null;
    // You can also programmatically close the info window
    if (this.infoWindow) {
      this.infoWindow.close();
    }
  }

  // Method to programmatically open info window
  openInfoWindow(marker: MapMarkerInfo): void {
    this.selectedMarker = marker;
    if (this.infoWindow) {
      this.infoWindow.open();
    }
  }

  // Method to get the Google Maps InfoWindow instance
  getInfoWindowInstance(): google.maps.InfoWindow | undefined {
    return this.infoWindow?.infoWindow;
  }

  // Method to customize info window programmatically
  customizeInfoWindow(): void {
    const instance = this.getInfoWindowInstance();
    if (instance) {
      instance.setOptions({
        maxWidth: 400,
        pixelOffset: new google.maps.Size(0, -15)
      });
    }
  }

  centerOnUser(): void {
    if (this.userLocation && this.isValidPosition(this.userLocation)) {
      this.mapCenter = { ...this.userLocation }; // Create new object to trigger change detection
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
    return specialist.profilePictureUrl || 'src/assets/avatar.svg';
  }

  // Helper method to show basic tag for unauthenticated users
  shouldShowBasicTag(): boolean {
    return !this.isAuthenticated;
  }
}
