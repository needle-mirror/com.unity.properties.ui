# Changelog
All notable changes to this package will be documented in this file.

## [1.3.1] - 2020-06-11
## Fixed
* Fixed custom inspectors for enum types not being considered. 
* Fixed `ObjectField` applying the `Texture2D` value as a background image, when `objectType` is set to `typeof(Texture2D)`.
* Fixed constant repainting when display a list.
* Fixed `InspectionContext` not being properly propagated with nested custom inspectors. 

### Changed
* Update `com.unity.properties` to version `1.3.1-preview`.
* Update `com.unity.serialization` to version `1.3.1-preview`.
  
## [1.3.0] - 2020-05-13
## Added
* Added support for using "." in `uxml` files to refer to the value being inspected in a custom inspector.
* Added support for automatically nesting `PropertyElement` using `bindingPath`.
* Added support for passing an inspection context to a ' PropertyElement'.
* Added overloads to `Inspector<TValue>.DoDefaultGUI[...]` to use the default drawer for a given property path, index or key.
* Added support for creating array instances. 

### Changed
* Update `com.unity.properties` to version `1.3.0-preview`.
* Update `com.unity.serialization` to version `1.3.0-preview`.

## Fixed
* Fixed specifying nested paths (i.e.: binding-path="path.to.field") in custom inspectors.
* Fixed binding to `UnityEngine.UIElements.Label` requiring a type converter to be registered.
* Fixed single line list items not shrinking properly.
* Fixed stack overflow issues that could happen when calling `DoDefaultGui()`.
* Fixed displaying type names for nested generic types.
* Fixed array fields not being editable.

## [1.2.0] - 2020-04-03
### Changed
* Update `com.unity.properties` to version `1.2.0-preview`.
* Update `com.unity.serialization` to version `1.2.0-preview`.

## [1.1.1] - 2020-03-20
### Fixed
* Fix `AttributeFilter` incorrectly being called on the internal property wrapper.

### Changed
* Update `com.unity.properties` to version `1.1.1-preview`.
* Update `com.unity.serialization` to version `1.1.1-preview`.

## [1.1.0] - 2020-03-11
### Fixed
* Fixed background color not being used when adding new collection items.
* Fixed readonly arrays being resizable from the inspector.

### Changed
* Update `com.unity.properties` to version `1.1.0-preview`.
* Update `com.unity.serialization` to version `1.1.0-preview`.

### Added
* Added the `InspectorAttribute`, allowing to put property attributes on both fields and properties.
* Added the `DelayedValueAttribute`, which works similarly to `UnityEngine.DelayedAttribute`, but can work with properties.
* Added the `DisplayNameAttribute`, which works similarly to `UnityEngine.InspectorNameAttribute`, but can work with properties.
* Added the `MinValueAttribute`, which works similarly to `UnityEngine.MinAttribute`, but can work with properties.
* Added built-in inspector for `LazyLoadReference`.

## [1.0.0] - 2020-03-02
### Changed
* ***Breaking change*** Complete API overhaul, see the package documentation for details.
