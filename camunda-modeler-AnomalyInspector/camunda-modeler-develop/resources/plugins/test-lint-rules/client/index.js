/**
 * Copyright Camunda Services GmbH and/or licensed to Camunda Services GmbH
 * under one or more contributor license agreements. See the NOTICE file
 * distributed with this work for additional information regarding copyright
 * ownership.
 *
 * Camunda licenses this file to you under the MIT; you may not use this file
 * except in compliance with the MIT License.
 */

import { registerClientPlugin } from 'camunda-modeler-plugin-helpers';

import { config, resolver } from '../.bpmnlintrc';

registerClientPlugin({ config, resolver }, 'lintRules.bpmn');
registerClientPlugin({ config, resolver }, 'lintRules.cloud-bpmn');
