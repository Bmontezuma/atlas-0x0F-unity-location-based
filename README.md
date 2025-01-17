# Location-Based Application

A simulation of the augmented reality experience using mobile GPS coordinates.

---

## Overview

This project provides a hands-on experience with location-based functionality in Unity. You'll learn to:

- Retrieve GPS location coordinates.
- Transform real GPS coordinates into Unity local positions.
- Calculate the distance between two coordinates.
- Create and position 3D objects at specific GPS coordinates (latitude and longitude).

---

## Learning Objectives

By the end of this project, you should be able to confidently explain:

1. How to get your GPS location coordinates.
2. How to transform real GPS coordinates into Unity local positions.
3. How to calculate the distance between two coordinates.
4. How to draw a 3D experience at specific GPS coordinates (latitude and longitude).

---

## Project Requirements

- **README.md**: Include this file at the root of your project folder.
- **Git Ignore**: Use Unity’s default `.gitignore` for the `0x0F-unity-location-based` directory.
- **Repository**: Host this project in a repository named `0x0F-unity-location-based`.
- **Organization**:
  - Scenes and assets (e.g., scripts) must be well-organized.
- **Documentation**:
  - Public classes and members must include XML documentation tags.
  - Private classes and members should be documented (no XML tags required).

### Unity Version

This project was developed using Unity **2020.3.24f1**.

---

## Tasks

### 0. Get Your GPS Coordinates

**Objective**: Initialize a Unity project and configure it to log device coordinates.

1. Create a new Unity project.
2. Import the following packages via the Unity Package Manager:
   - **ARCore XR Plugin**
   - **AR Foundation**
3. Create a new scene with UI elements to log:
   - **Latitude**
   - **Longitude**
   - **Altitude**

**Repository**:  
[GitHub repository: atlas-0x0F-unity-location-based](https://github.com/your-repo-url)

---

### 1. Set Up a New Coordinate

**Objective**: Add functionality to store and retrieve GPS coordinates.

1. Create a UI button to store the current GPS coordinates.
2. Create another UI button to retrieve the updated coordinates after moving (optional if using `Update()`).

**Repository**:  
[GitHub repository: atlas-0x0F-unity-location-based](https://github.com/your-repo-url)

---

### 2. Calculate Distance Between Two Coordinates

**Objective**: Compute and display the distance between two points.

1. Create a UI button to calculate the distance (in meters) between:
   - A stored coordinate.
   - The current coordinate.
2. Log the calculated distance on the UI.

**Repository**:  
[GitHub repository: atlas-0x0F-unity-location-based](https://github.com/your-repo-url)

---

### 3. Convert GPS Coordinates to Unity Local Position

**Objective**: Translate GPS coordinates into Unity's local coordinate system.

1. Use a third-party library or API to perform the conversion.
2. Recommended third-party tools: Any suitable package of your choice.

**Repository**:  
[GitHub repository: atlas-0x0F-unity-location-based](https://github.com/your-repo-url)

---

### 4. Draw a 3D Mesh

**Objective**: Instantiate 3D objects at specific locations.

1. Create a 3D object at your current GPS position.
2. Instantiate another 3D object at a target destination coordinate.

**Repository**:  
[GitHub repository: atlas-0x0F-unity-location-based](https://github.com/your-repo-url)

---

### 5. Draw a 3D Marker

**Objective**: Add markers with text at designated GPS coordinates.

1. Instantiate a 3D object with text at:
   - Your current position.
   - Your destination position.

**Repository**:  
[GitHub repository: atlas-0x0F-unity-location-based](https://github.com/your-repo-url)

---

## Additional Resources

- [Unity Documentation](https://docs.unity3d.com/)
- [ARCore Developer Guide](https://developers.google.com/ar)
- [AR Foundation Documentation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest)

---
