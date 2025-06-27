# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added

### Changed

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