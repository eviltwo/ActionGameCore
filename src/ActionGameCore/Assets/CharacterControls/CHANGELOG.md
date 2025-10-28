# Changelog

## [Development]

### Changed

- Rename CharacterMoveInput to CharacterMoveInputRelay.

### Added

- Added bot input samples.

## [0.12.0]

### Changed

- Changed so that each module and input data now strongly depend on the Input System’s actions.

### Added

- Added SphereCast for ground check.

## [0.11.0]

### Changed

- Changed the module to inherit from CharacterModuleBase.
- Changed the movement process to the Module class.

### Added

- Added a parameter to make the Pull-up module operable on the ground.
- Added dash.

### Fixed

- Fixed a bug where it would hop on slopes.
- Fixed friction calculation.
- Fixed max velocity in air.
- Fixed the issue where the jump height was slightly low.

## [0.10.1] - 2024-08-08

### Changed

- Changed walk on stairs to smooth.
- changed walk on narrow bridge to smooth.
- Changed input setup for modules.
- Changed move control in air to more fast.
- Changed wall check for pull up movement.
- Changed to directly play the pull up animator state.
- Changed to match the speed of the grabbed object.

### Fixed

- Fixed spring and damper issues.
- Fixed issue where the height was too high when jumping before landing on the ground.

## [0.10.0] - 2024-08-04

### Changed

- The character's movement process has been subdivided into the Module class.

### Added

- Added Pull up module.
- Added character animator controller for each modules.

## [0.9.0] - 2024-07-07

### Changed

- Changed to use PlayerInput component

## [0.6.1] - 2024-03-06

### Fixed

- Fix input action enabled.

## [0.6.0] - 2024-03-06

### Changed

- Change parameter type of input to InputActionReference.

## [0.5.4] - 2024-03-04

### Fixed

- Fix error with default input action asset.

## [0.5.3] - 2024-03-03

### Fixed

- Clear jump input flag every frame.

## [0.5.2] - 2024-03-03

### Added

- Jump and air walk.

## [0.5.1] - 2024-03-03

### Added

- Add horizontal velocity by floor movement.

### Fixed

- Fix leg suspension for elevator.

## [0.5.0] - 2024-02-27

### Added

- Add character stay and walk.
