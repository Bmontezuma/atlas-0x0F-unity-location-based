# GeoAR Project
![Image](https://i.postimg.cc/NfrCTsVG/thumbnail.jpg)

## Overview
This project is an **Augmented Reality (AR) application** that integrates **geolocation** to place virtual objects at real-world GPS coordinates. Using **Unity AR Foundation, ARCore, and GPS services**, the application allows users to visualize spatial relationships between real and virtual objects, including 3D models of **Earth and the Moon**.

## Features
- **Real-time GPS Tracking**: Displays current latitude, longitude, and altitude.
- **Location Storage**: Users can store a location as a reference point.
- **Distance Calculation**: Computes the distance between the stored location and the user's current position using the **Haversine formula**.
- **AR Object Placement**: Spawns 3D models (Earth and Moon) at designated GPS locations.
- **UI Display**: Provides a user-friendly interface with tracking indicators and distance measurements.

## Technologies Used
- **Unity** (AR Foundation, ARCore XR Plugin)
- **GPS Services** (Unity Location Service, Android Location API)
- **GPSEncoder** (Third-party tool for GPS-to-Unity coordinate conversion)
- **C# Scripting** (For managing AR tracking, GPS retrieval, and UI interactions)
- **TextMeshPro** (For dynamic UI text updates)

## Installation & Setup
1. **Clone the repository**:
   ```sh
   git clone https://github.com/your-username/atlas-0x0F-unity-location-based.git
   ```
2. **Open the project in Unity** (Ensure Unity version supports AR Foundation & ARCore).
3. **Install Required Packages** via **Unity Package Manager**:
   - ARCore XR Plugin
   - AR Foundation
   - TextMeshPro (for UI text handling)
4. **Enable Location Services** on your device.
5. **Grant necessary permissions** for Camera and Fine Location on Android.
6. **Run the project** on a compatible AR-supported device.

## Usage
### **Getting Started**
- Launch the app and allow necessary permissions.
- The UI will display GPS coordinates and AR tracking status.

### **Key Functionalities**
| Feature | Description |
|---------|------------|
| **Get Current Coordinates** | Retrieves and displays latitude, longitude, and altitude. |
| **Set Destination Coordinates** | Stores a reference GPS location for comparison. |
| **Calculate Distance** | Computes real-world distance between the stored and current location. |
| **Spawn AR Objects** | Instantiates 3D models at the respective locations. |
| **GPS & AR Status** | Indicates if tracking is active and GPS is working. |

## Code Implementation
### **Core Scripts**
- `ObjectPlacementManager.cs` – Handles object placement at GPS-based locations.
- `UnifiedGPSManager.cs` – Manages GPS retrieval, UI updates, and AR session handling.
- `GPSEncoder` – Converts GPS coordinates into Unity local position.

### **Distance Calculation (Haversine Formula)**
```csharp
private float HaversineDistance(Vector2 coord1, Vector2 coord2)
{
    float R = 6371000f; // Earth's radius in meters
    float dLat = Mathf.Deg2Rad * (coord2.x - coord1.x);
    float dLon = Mathf.Deg2Rad * (coord2.y - coord1.y);

    float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
              Mathf.Cos(Mathf.Deg2Rad * coord1.x) * Mathf.Cos(Mathf.Deg2Rad * coord2.x) *
              Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
    
    float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
    return R * c; // Distance in meters
}
```

## Challenges & Solutions
### **1. GPS & AR Tracking Conflicts**
- **Issue**: Multiple scripts accessing GPS caused incorrect positioning.
- **Solution**: Ensured only one script managed GPS retrieval at a time.

### **2. Positioning Accuracy Issues**
- **Issue**: Objects were not spawning at expected locations.
- **Solution**: Implemented `GPSEncoder.GPSToUCS()` with a fixed reference origin.

### **3. Incorrect Distance Measurements**
- **Issue**: Initial calculations did not match expected real-world distances.
- **Solution**: Applied the **Haversine formula** for more accurate geospatial distance.

## Future Improvements
- **Persistent AR Anchors**: Save object placements for multi-session experiences.
- **Enhanced GPS Accuracy**: Integrate **Mapbox API** or **Niantic Lightship VPS**.
- **Multi-User Shared AR**: Allow multiple users to interact with the same AR space.
- **Dynamic Environmental Data**: Use **real-time weather and terrain mapping**.

## License
This project is open-source and available under the **MIT License**.

## Acknowledgments
- **Niantic Lightship** (for geolocation AR inspiration)
- **GPSEncoder** by Michael Taylor (for coordinate conversion in Unity)
- **Unity & ARCore Teams** (for AR tracking frameworks)

## Contact
For contributions, suggestions, or issues, feel free to open an **Issue** or **Pull Request** in the GitHub repository!

---

**🚀 Experience the fusion of AR and geolocation with GeoAR!**
