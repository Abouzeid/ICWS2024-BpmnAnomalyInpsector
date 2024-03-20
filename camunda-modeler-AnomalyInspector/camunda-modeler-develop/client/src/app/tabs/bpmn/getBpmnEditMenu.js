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
  getCanvasEntries,
  getAlignDistributeEntries,
  getCopyCutPasteEntries,
  getDefaultCopyCutPasteEntries,
  getDefaultUndoRedoEntries,
  getDiagramFindEntries,
  getSelectionEntries,
  getToolEntries,
  getUndoRedoEntries,
  getColorEntries
} from '../getEditMenu';


export function getBpmnEditMenu(state) {
  const {
    defaultCopyCutPaste,
    defaultUndoRedo
  } = state;

  const undoRedoEntries = defaultUndoRedo
    ? getDefaultUndoRedoEntries(true)
    : getUndoRedoEntries(state);

  const copyCutPasteEntries = defaultCopyCutPaste
    ? getDefaultCopyCutPasteEntries(true)
    : getCopyCutPasteEntries(state);

  return [
    undoRedoEntries,
    copyCutPasteEntries,
    getToolEntries(state),
    getAlignDistributeEntries(state),
    getColorEntries(state),
    getDiagramFindEntries(state),
    [
      ...getCanvasEntries(state),
      ...getSelectionEntries(state)
    ]
  ];
}
