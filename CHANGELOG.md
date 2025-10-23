# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added

- `LinkMarkupHandler` provides a way for TMP links to open content inside of samples
    - Added links throughout various samples in the Yarn.
- `SetAppearance` to the `CharacterAppearance` class, allowing overriding the default colours on the fly

### Changed

- Fixed an issue where fallback fonts were not configured correctly
- DialogueInteractable now attempts to pre-warm some state to avoid framerate hitching that can occur when the interaction indicator first appears.
- Fixed an issue where the Unity Localization sample was missing table references.
- Fixed an issue where the font assets were not configured to use the correct fallback font for Simplified Chinese characters.
- Renamed the misspelt `SpriteReplacmentMarkerProcessor` to `SpriteReplacementMarkerProcessor`
- `TriggerArea` now is aware starting dialogue is asynchronous
- `DialogueInteractable` is now aware starting dialogue is asynchronous
- NPC in Feature Tour now has the correct node
    - was running a commands explanation
    - now runs a functions explanation

### Removed

## [3.0.4] 2025-06-27

### Added

- Added AnimationState attribute, which allows choosing an animation state from a dropdown
- Added SimpleCharacter2D character controller
- Added 2D sprites
- Added scifi models

### Changed

- Fixed compiler warnings in SimpleCharacterInputAxis
- DialogueInteractable no longer throws 'X is not a valid node name' if the dialogue runner has no Yarn Project

## [3.0.3] 2025-06-21

### Added

- Added support for Unity Input System for player movement
- Added SampleInputActions and configured scenes to use them (they'll fall back to the keyboard if the Input System isn't installed)
- Added additional outdoor environment props

## [3.0.2] 2025-06-13

### Changed

- Fixed broken materials in BackgroundChatter and the mouth shader in Unity 2022.
- Fixed missing references to UniTask's assembly in individual samples.
- Fixed a bug in imported font assets that could cause the Unity Editor to crash in Unity 2022.
- Fixed cowboy hat asset import settings.
- Fixed a texture seam with the skybox.

## [3.0.0] 2025-05-16

Initial release.