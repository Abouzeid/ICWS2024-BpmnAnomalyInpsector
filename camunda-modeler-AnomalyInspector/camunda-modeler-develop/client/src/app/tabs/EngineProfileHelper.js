/**
 * Copyright Camunda Services GmbH and/or licensed to Camunda Services GmbH
 * under one or more contributor license agreements. See the NOTICE file
 * distributed with this work for additional information regarding copyright
 * ownership.
 *
 * Camunda licenses this file to you under the MIT; you may not use this file
 * except in compliance with the MIT License.
 */

import {
  engineProfilesEqual,
  isKnownEngineProfile
} from './EngineProfile';

export default class EngineProfileHelper {
  constructor({ get, set, getCached, setCached }) {
    this._get = get;
    this._set = set;
    this._getCached = getCached;
    this._setCached = setCached;
  }

  get() {
    const engineProfile = this._get();

    if (!isKnownEngineProfile(engineProfile)) {
      throw new Error(getUnknownEngineProfileErrorMessage(engineProfile));
    }

    return engineProfile;
  }

  set(engineProfile) {
    this._set(engineProfile);

    this.setCached(engineProfile);
  }

  getCached() {
    const { engineProfile } = this._getCached();

    return engineProfile;
  }

  setCached(engineProfile) {
    const { engineProfile: cachedEngineProfile } = this._getCached();

    if (!engineProfilesEqual(engineProfile, cachedEngineProfile)) {
      this._setCached({
        engineProfile
      });
    }
  }
}

function getUnknownEngineProfileErrorMessage(engineProfile = {}) {
  const {
    executionPlatform = '<no-execution-platform>',
    executionPlatformVersion = '<no-execution-platform-version>'
  } = engineProfile;

  return `An unknown execution platform (${ executionPlatform } ${ executionPlatformVersion }) was detected.`;
}