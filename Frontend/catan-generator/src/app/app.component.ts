import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HttpClient, HttpClientModule,HttpParams } from '@angular/common/http';
import { CommonModule } from '@angular/common'; // Import CommonModule
import { Tile } from '../models/tile.model';
import { FormsModule } from '@angular/forms';




@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, HttpClientModule, CommonModule,FormsModule], // Add CommonModule here
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'catan-generator';
  http = inject(HttpClient);

  tiles: Tile[] = [];
  rows: Tile[][] = [];
  orderOfPlay: string[] = [];
  sixAndEightCanTouch = true;
  sameNumbers = true;
  sameTypes = true;

  GenerateBoard(): void {
    // Construct query parameters based on checkbox values
    const params = new HttpParams()
      .set('sixAndEightCanTouch', this.sixAndEightCanTouch.toString())
      .set('sameNumbers', this.sameNumbers.toString())
      .set('sameTypes', this.sameTypes.toString());
  
    // Send HTTP GET request with parameters
    this.http.get<Tile[]>('https://localhost:7154/api/Tile', { params }).subscribe({
      next: (data: Tile[]) => {
        this.tiles = data;
        console.log('Tiles fetched successfully:', this.tiles);
        this.generateRows(); // Generate rows after fetching data
      },
      error: (err) => {
        console.error('Error fetching tiles:', err);
      }
    });
  }
  
  calculatePercentages() {
    const diceFrequencyMap: { [key: string]: number } = {
      2: 1, 3: 2, 4: 3, 5: 4, 6: 5, 8: 5, 9: 4, 10: 3, 11: 2, 12: 1
    };
  
    let frequencyCount: { [key: string]: number } = {};
  
    // Exclude Desert and calculate the frequency
    this.tiles
      .filter(tile => tile.type !== 'Desert')  // Filter out Desert tiles
      .forEach(tile => {
        if (!frequencyCount[tile.type]) {
          frequencyCount[tile.type] = 0;
        }
        const diceValue = diceFrequencyMap[tile.diceNumber.toString()] || 0;
        frequencyCount[tile.type] += diceValue;
      });
  
    let percentages: { type: string, percentage: number }[] = [];
  
    for (const type in frequencyCount) {
      const percentage = (frequencyCount[type] / 36) * 100;
      percentages.push({ type, percentage: parseFloat(percentage.toFixed(2)) });
    }
  
    // Sort percentages by descending order
    percentages.sort((a, b) => b.percentage - a.percentage);
  
    return percentages;
  }
  
  

  generateRows() {
    const rowSizes = [3, 4, 5, 4, 3];
    let index = 0;

    this.rows = rowSizes.map(size => {
      const row = this.tiles.slice(index, index + size);
      index += size;
      return row;
    });
  }

  getImageUrl(type: string): string {
    const imageMap: { [key: string]: string } = {
      'Mountain': 'images/mountain.png',
      'Hill': 'images/hill.png',
      'Forest': 'images/forest.png',
      'Field': 'images/field.png',
      'Pasture': 'images/pasture.png',
      'Desert': 'images/desert.png'
    };
    return imageMap[type] || 'default.png'; // Provide a default image if type is unknown
  }

  getMarginForRow(index: number): { [key: string]: string } {
    const margins = [
      {'margin-top': '30px'},
      {'margin-top': '-7.5%'},
      {'margin-top': '-7.5%'},
      {'margin-top': '-7.5%'},
      {'margin-top': '-7.5%'}
    ];
    return margins[index] || { 'margin-left': '0px', 'margin-bottom': '0px', 'margin-top': '0px', 'margin-right': '0px' }; // Default to '0px'
  }

  GenerateOrderOfPlay(): void {
    this.http.get<string[]>('https://localhost:7154/api/Tile/OrderOfPlay').subscribe({
      next: (data: string[]) => {
        this.orderOfPlay = data;
        console.log('Order of play fetched successfully:', this.orderOfPlay);
      },
      error: (err) => {
        console.error('Error fetching tiles:', err);
      }
    });
  }

  getPlayerColorClass(player: string): string {
    switch (player.toLowerCase()) {
      case 'white':
        return 'player-white';
      case 'blue':
        return 'player-blue';
      case 'red':
        return 'player-red';
      case 'orange':
        return 'player-orange';
      default:
        return '';
    }
  }

  openOptions() {
    const modal = document.getElementById('optionsModal');
    if (modal) {
      modal.style.display = 'flex';
    }
  }
  
  closeOptions() {
    const modal = document.getElementById('optionsModal');
    if (modal) {
      modal.style.display = 'none';
    }
  }
  
  
}
