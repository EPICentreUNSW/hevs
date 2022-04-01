# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2021-04-12
This is the first release of HEVS 2.0.0 and contains major API-breaking changes from previous versions.

### Major Changes
- HEVS.Core replaces HEVS.Application/Configuration and also incorporates functionality that was previously encapsulated by HEVS.PlatformConfig/NodeConfig and other deprecated classes.
- Logical split of configuration data and run-time functionality.
- HEVS.TrackerManager replaced by HEVS.Tracker. Specific tracker type components (i.e. VRPNTracker/OSCTracker/XRTracker/MouseTracker) have been removed and instead replaced with non-component-based devices that are used by HEVS.Tracker internally. Custom tracker devices now possible.
- Specific HEVS.ClusterVariable types (i.e. ClusterBool, ClusterVector3) now replaced by a templated HEVS.ClusterVar\<T\> type.
- HEVS.RPCManager renamed to HEVS.RPC, and HEVS.RPCManager.CallOnAll() method renamed to HEVS.RPC.Call(). Methods now take a handle to the method to call, rather than a string.

### Added
- Data broadcasting possible via HEVS.Cluster.BroadcastData() method.
- HEVS.Core opens a port for receiving OSC packets that can be directed to HEVS.OSCReceiver Components.