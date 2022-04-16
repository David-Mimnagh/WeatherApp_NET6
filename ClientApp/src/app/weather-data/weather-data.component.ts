import { Component, ElementRef, Inject, ViewChild } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { fromEvent } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

@Component({
  selector: 'app-weather-data',
  templateUrl: './weather-data.component.html',
  styleUrls: ['./weather-data.component.css']
})
export class WeatherDataComponent {
  public forecast!: WeatherForecast;
  public citiesList!: CityCondensed[];
  public selectedCityName!: string;
  httpClient: HttpClient;
  baseUrl: string;
  loading: boolean;
  selectedCity: boolean;
  useMetric: boolean;
  errorMessage?: string;
  @ViewChild('cityInput', { static: false }) locationSearchBox!: ElementRef;
  ngAfterViewInit() {
    fromEvent(this.locationSearchBox.nativeElement, 'keyup').pipe(
      debounceTime(2000) // 2 seconds
    ).subscribe((ev: any) => {
      if (!this.selectedCity) {
        //@ts-ignore
        this.getCities(ev.target.value)
      }
      //resolve an issue where the selection of a city fired the debounce.
      this.selectedCity = false;
    });
    const input = document.querySelector('#locationSearchInput')
    //@ts-ignore
    input.onchange = (e) => {
      this.selectedCity = true;
      var opts = document.getElementById('city-list')?.childNodes ?? [];
      for (var i = 0; i < opts.length; i++) {
        //@ts-ignore
        if (opts[i].value === e.target.value) {
          this.selectedCityName = e.target.value;
          //@ts-ignore
          this.getWeatherForecast(null, null, opts[i].innerText);
          break;
        }
      }
    }
  }
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.httpClient = http;
    this.baseUrl = baseUrl;
    this.loading = false;
    this.selectedCity = false;
    this.useMetric = false;
    this.errorMessage = undefined;
  }
  getWeatherForecast(latitude: number | null = null, longitude: number | null = null, cityKey: string | null = null) {
    this.errorMessage = undefined;

    if (latitude && !longitude || !latitude && longitude) {
      this.errorMessage = "Please provide both a latitute and longitude.";
    }
    let queryParams = new HttpParams();

    if (latitude && longitude) {
      queryParams = queryParams.append("latitude", latitude.toString());
      queryParams = queryParams.append("longitude", longitude.toString());
    }

    if (cityKey) {
      queryParams = queryParams.append("cityKey", cityKey);
    }
    queryParams = queryParams.append("useMetric", this.useMetric.toString());
    this.loading = true;
    this.httpClient.get<WeatherForecast>(this.baseUrl + 'weatherforecast/getweatherinfo', { params: queryParams }).subscribe(result => {
      this.forecast = result;
      this.selectedCityName = this.forecast.locationName === cityKey ? this.selectedCityName : this.forecast.locationName;
      this.loading = false;
    }, error => {
      this.loading = false;
      if (error && error.message) {
        console.error(error);
        this.errorMessage = error.message;
      }
    });
  }
  getCurrentLocation() {
    navigator.geolocation.getCurrentPosition((geoLoc) => this.getWeatherForecast(geoLoc.coords.latitude, geoLoc.coords.longitude))
  }
  getCities(location: string | null = null) {
    this.errorMessage = undefined;
    if (!location) {
      console.log("Error making getCities call, location is undefined");
      return;
    }
    const queryParams = new HttpParams().append("location", location);
    this.httpClient.get<CityCondensed[]>(this.baseUrl + 'city/getcities', { params: queryParams }).subscribe(result => {
      this.citiesList = result;
      this.loading = false;
    }, error => {
      this.loading = false;
      if (error && error.message) {
        console.error(error);
        this.errorMessage = error.message;
      }
    });
  }
}
interface WeatherForecast {
  date: string;
  locationName: string;
  temperatureMin: number;
  temperatureMax: number;
  summaryText: string;
  percipitation: string;
  forecastLink: string;
}
interface CityCondensed {
  key: string;
  name: number;
}
