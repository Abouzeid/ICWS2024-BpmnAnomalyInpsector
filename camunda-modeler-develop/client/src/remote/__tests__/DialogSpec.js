/**
 * Copyright Camunda Services GmbH and/or licensed to Camunda Services GmbH
 * under one or more contributor license agreements. See the NOTICE file
 * distributed with this work for additional information regarding copyright
 * ownership.
 *
 * Camunda licenses this file to you under the MIT; you may not use this file
 * except in compliance with the MIT License.
 */

import Dialog from '../Dialog';

import { Backend } from '../../app/__tests__/mocks';

describe('dialog', function() {

  it('#showOpenFilesDialog', function() {

    // given
    const sendSpy = (type, opts) => {

      // then
      expect(type).to.equal('dialog:open-files');

      expect(opts).to.eql(options);
    };

    const backend = new Backend({ send: sendSpy });
    const dialog = new Dialog(backend);

    const options = {
      defaultPath: 'foo',
      filter: {
        extensions: [ 'foo' ],
        name: 'foo'
      },
      title: 'Foo'
    };

    // when
    dialog.showOpenFilesDialog(options);
  });


  it('#showOpenFileErrorDialog', function() {

    // given
    const sendSpy = (type, opts) => {

      // then
      expect(type).to.equal('dialog:show');

      expect(opts).to.eql({
        type: 'error',
        title: 'Unrecognized file format',
        buttons: [
          { id: 'cancel', label: 'Close' }
        ],
        message: 'The file "foo" is not a foo, bar or baz file.'
      });
    };

    const backend = new Backend({ send: sendSpy });
    const dialog = new Dialog(backend);

    const options = {
      file: {
        name: 'foo'
      },
      types: [ 'foo', 'bar', 'baz' ]
    };

    // when
    dialog.showOpenFileErrorDialog(options);
  });


  it('#showSaveFileDialog', function() {

    // given
    const sendSpy = (type, opts) => {

      // then
      expect(type).to.equal('dialog:save-file');

      expect(opts).to.eql(options);
    };

    const backend = new Backend({ send: sendSpy });
    const dialog = new Dialog(backend);

    const options = {
      defaultPath: 'foo',
      filter: {
        extensions: [ 'foo' ],
        name: 'foo'
      },
      title: 'Foo'
    };

    // when
    dialog.showSaveFileDialog(options);
  });


  it('#showSaveFileErrorDialog', function() {

    // given
    const sendSpy = (type, opts) => {

      // then
      expect(type).to.equal('dialog:show');

      expect(opts).to.eql(options);
    };

    const backend = new Backend({ send: sendSpy });
    const dialog = new Dialog(backend);

    const options = {
      buttons: [
        { id: 'foo', label: 'Foo' }
      ],
      message: 'Foo',
      title: 'Foo',
      type: 'error'
    };

    // when
    dialog.showSaveFileErrorDialog(options);
  });


  it('#show', function() {

    // given
    const sendSpy = (type, opts) => {

      // then
      expect(type).to.equal('dialog:show');

      expect(opts).to.eql(options);
    };

    const backend = new Backend({ send: sendSpy });
    const dialog = new Dialog(backend);

    const options = {
      type: 'info',
      title: 'Foo',
      message: 'Foo!',
      buttons: [
        { id: 'foo', label: 'Foo' }
      ]
    };

    // when
    dialog.show(options);
  });


  it('#showCloseFileDialog', function() {

    // given
    const sendSpy = (type, opts) => {

      // then
      expect(type).to.equal('dialog:show');

      const buttons = [
        { id: 'save', label: 'Save' },
        { id: 'discard', label: 'Don\'t Save' },
        { id: 'cancel', label: 'Cancel' },
      ];

      expect(opts).to.eql({
        buttons,
        defaultId: 0,
        type: 'question',
        title: 'Close File',
        message: 'Save changes to "foo" before closing?',
      });
    };

    const backend = new Backend({ send: sendSpy });
    const dialog = new Dialog(backend);

    const options = {
      name: 'foo'
    };

    // when
    dialog.showCloseFileDialog(options);
  });


  it('#showEmptyFileDialog', function() {

    // given
    const sendSpy = (type, opts) => {

      // then
      expect(type).to.equal('dialog:show');

      const isLinux = process.platform === 'linux';

      let buttons;

      if (isLinux) {
        buttons = [
          { id: 'cancel', label: 'Cancel' },
          { id: 'create', label: 'Create' }
        ];
      } else {
        buttons = [
          { id: 'create', label: 'Create' },
          { id: 'cancel', label: 'Cancel' }
        ];
      }

      expect(opts).to.eql({
        buttons,
        defaultId: isLinux ? 1 : 0,
        type: 'info',
        title: 'Empty FOO file',
        message: 'The file "foo" is empty.\nWould you like to create a new FOO diagram?'
      });
    };

    const backend = new Backend({ send: sendSpy });
    const dialog = new Dialog(backend);

    const options = {
      file: {
        name: 'foo'
      },
      type: 'foo'
    };

    // when
    dialog.showEmptyFileDialog(options);
  });


  it('#showFileExplorerDialog', function() {

    // given
    const sendSpy = (type, opts) => {

      // then
      expect(type).to.equal('dialog:open-file-explorer');

      expect(opts).to.eql(options);
    };

    const backend = new Backend({ send: sendSpy });
    const dialog = new Dialog(backend);

    const options = {
      path: 'foo'
    };

    // when
    dialog.showFileExplorerDialog(options);
  });


  it('#showReloadDialog', function() {

    // given
    const sendSpy = (type, opts) => {

      // then
      expect(type).to.equal('dialog:show');

      const isLinux = process.platform === 'linux';

      let buttons;

      if (isLinux) {
        buttons = [
          { id: 'cancel', label: 'Cancel' },
          { id: 'reload', label: 'Continue without saving' },
          { id: 'save', label: 'Save' }
        ];
      } else {
        buttons = [
          { id: 'save', label: 'Save' },
          { id: 'reload', label: 'Continue without saving' },
          { id: 'cancel', label: 'Cancel' }
        ];
      }

      expect(opts).to.eql({
        buttons,
        defaultId: isLinux ? 2 : 0,
        type: 'question',
        title: 'Reload Modeler',
        message: 'Reloading the modeler will discard all unsaved changes. Do you want to save before reloading?'
      });
    };

    const backend = new Backend({ send: sendSpy });
    const dialog = new Dialog(backend);

    // when
    dialog.showEmptyFileDialog();

  });
});
