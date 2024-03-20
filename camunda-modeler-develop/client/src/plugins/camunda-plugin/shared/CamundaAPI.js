/* eslint-disable license-header/header */
/* eslint-disable spaced-comment */
/* eslint-disable indent */
/**
 * Copyright Camunda Services GmbH and/or licensed to Camunda Services GmbH
 * under one or more contributor license agreements. See the NOTICE file
 * distributed with this work for additional information regarding copyright
 * ownership.
 *
 * Camunda licenses this file to you under the MIT; you may not use this file
 * except in compliance with the MIT License.
 */

import AuthTypes from './AuthTypes';

import debug from 'debug';

const FETCH_TIMEOUT = 5000;

const log = debug('CamundaAPI');


export default class CamundaAPI {

  constructor(endpoint) {

    this.baseUrl = normalizeBaseURL(endpoint.url);

    this.authentication = this.getAuthentication(endpoint);
  }

  async deployDiagram(diagram, deployment) {
    const {
      name,
      tenantId,
      attachments = []
    } = deployment;

    var hasAnomaly = await this.checkArtifactAnomaly(diagram);

    // var data = new FormData();
    //data.append('file', new Blob([ diagram.contents ]), diagram.name);
    //var xhr = new XMLHttpRequest();

    //xhr.withCredentials = true;
    //xhr.addEventListener('readystatechange', function() {
    //  if (this.readyState === 4) {
    //    console.log(this.responseText);
    //  }
    //});

    //xhr.open('POST', 'http://localhost:5159/FileUpload/UploadBPMN');
    //xhr.send(data);

    if (!hasAnomaly) {

      // Origina Deployment code

    const form = new FormData();

    form.append('deployment-name', name);
    form.append('deployment-source', 'Camunda Modeler');

    // make sure that we do not re-deploy already existing deployment
    form.append('enable-duplicate-filtering', 'true');

    if (tenantId) {
      form.append('tenant-id', tenantId);
    }

    const diagramName = diagram.name;

    const blob = new Blob([ diagram.contents ], { type: 'text/xml' });

    form.append(diagramName, blob, diagramName);

    attachments.forEach(file => {
      form.append(file.name, new Blob([ file.contents ]), file.name);
    });

    const response = await this.fetch('/deployment/create', {
      method: 'POST',
      body: form
    });

    if (response.ok) {

      const {
        id,
        deployedProcessDefinitions
      } = await response.json();

      return {
        id,
        deployedProcessDefinitions,
        deployedProcessDefinition: Object.values(deployedProcessDefinitions || {})[0]
      };
    }

    const body = await this.parse(response);

    throw new DeploymentError(response, body);}
  }

  async checkArtifactAnomaly(diagram) {
  var data = new FormData();
  data.append('file', new Blob([ diagram.contents ]), diagram.name);

  try {
    const response = await fetch('http://localhost:5159/FileUpload/UploadBPMN?isPrevApproach=false', {
      method: 'POST',
      body: data,
      credentials: 'include' // for CORS and sending cookies, similar to xhr.withCredentials = true;
    });

    const jsonResponse = await response.json();

   this.OurApproachTableWithData(jsonResponse, 'error');

     // this.PrevMethodRenderTable(jsonResponse);

    if (!response.ok) {

     // this.tempAlert(`Artifact_Anomaly_Detector: ${JSON.stringify(jsonResponse)}`, 'error');

      //this.displayJsonAsTable(jsonResponse, 'error');
      return true;

      // throw new DeploymentError(response, await this.parse(response));
    }
    return false;

    //throw new AnomalyError('Some anomaly details', JSON.stringify(jsonResponse));

    // alert(`Artifact_Anomaly_Detector: ${JSON.stringify(jsonResponse)}`);

    //this.tempAlert(`Artifact_Anomaly_Detector: ${JSON.stringify(jsonResponse)}`, 'error', 10000);
  } catch (error) {

    //this.tempAlert(`Error: ${error.message}`, 'error');
    alert(`Artifact_Anomaly_Detector: ${error}`);
  }
  }

  async tempAlert(msg, type) {

    var el = document.createElement('div');
    el.setAttribute('style', `
    position: fixed; 
    bottom: 10%; 
    left: 50%; 
    transform: translateX(-50%);
    background-color: ${type === 'error' ? '#ffdddd' : '#ddffdd'}; 
    color: ${type === 'error' ? '#d8000c' : '#006400'}; 
    font-size: 15px; 
    padding: 20px; 
    border-radius: 5px; 
    box-shadow: 0px 0px 10px #999; 
    z-index: 1000;
    border: ${type === 'error' ? '1px solid #d8000c' : '1px solid #006400'};
    min-width: 300px; /* Minimum width, but can grow */
    max-width: 80%; /* Prevents the div from becoming too wide */
    box-sizing: border-box;
    white-space: normal; /* Ensures text wraps */
    overflow-wrap: break-word; /* Breaks words to prevent overflow */
`);

    el.innerHTML = `<span style="float:right; cursor:pointer; color:#000;">&times;</span><p style="margin:0; text-align:left;">${msg}</p>`;

    // Add style for the close button for consistency
    var closeButton = el.querySelector('span');
    closeButton.style.marginRight = '10px';
    closeButton.style.marginTop = '5px';

    document.body.appendChild(el);

    // Close button functionality
    closeButton.onclick = function() {
      document.body.removeChild(el);
    };

  }


  async displayTableWithData(data, type) {
  var el = document.createElement('div');
  el.setAttribute('style', `
    position: fixed; 
    bottom: 10%; 
    left: 50%; 
    transform: translateX(-50%);
    background-color: ${type === 'error' ? '#ffdddd' : '#ddffdd'}; 
    color: ${type === 'error' ? '#d8000c' : '#006400'}; 
    font-size: 10px; 
    padding: 20px; 
    border-radius: 5px; 
    box-shadow: 0px 0px 10px #999; 
    z-index: 1000;
    border: ${type === 'error' ? '1px solid #d8000c' : '1px solid #006400'};
    min-width: 300px;
    max-width: 80%;
    box-sizing: border-box;
    overflow-x: auto; /* Allow horizontal scrolling for wide tables */
    `);

  // Creating table structure
  //var table = '<table style="width: 100%; border-collapse: collapse;"><tr style="background-color: #f2f2f2;"><th>Artifact_id</th><th>AndNode_Id</th><th>ReadNodes</th><th>WriteNodes</th><th>KillNodes</th></tr>';
    let table = '<table style="border-collapse: collapse; width: 100%;">';
    table += '<tr style="background-color: #f2f2f2;"><th>Artifact_id</th><th>AndNode_Id</th><th>ReadNodes</th><th>WriteNodes</th><th>KillNodes</th><th>Level</th><th>OfType</th></tr>';

  // Loop through each data item and add table rows
  data.forEach(item => {
    table += `<tr>
            <td>${item.Artifact_id || ''}</td>
            <td>${item.AndNode_Id || ''}</td>
            <td>${item.ReadNodes ? item.ReadNodes.join(', ') : ''}</td>
            <td>${item.WriteNodes ? item.WriteNodes.join(', ') : ''}</td>
            <td>${item.KillNodes ? item.KillNodes.join(', ') : ''}</td>
            <td>${item.Level || ''}</td>
            <td>${item.AnomalyType || ''}</td>
          </tr>`;
  });

  table += '</table>';
  el.innerHTML = `<span style="float:right; cursor:pointer; color:#000;">&times;</span><div>${table}</div>`;

  var closeButton = el.querySelector('span');
  closeButton.style.marginRight = '10px';
  closeButton.style.marginTop = '5px';

  document.body.appendChild(el);

  closeButton.onclick = function() {
    document.body.removeChild(el);
  };

  }

  async OurApproachTableWithData(data, type) {
    var el = document.createElement('div');
    el.setAttribute('style', `
    position: fixed; 
    bottom: 10%; 
    left: 50%; 
    transform: translateX(-50%);
   background-color: #fff;
    color: #333;
    font-size: 13px; 
    padding: 20px; 
    border-radius: 5px; 
    box-shadow: 0px 0px 10px #999; 
    z-index: 1000;  
    min-width: 300px;
    max-width: 80%;
    box-sizing: border-box;
    overflow-x: auto;
  `);

    let table = '<table style="border-collapse: collapse; margin: 15px 0; font-size: 0.9em; min-width: 400px; width: 100%; box-shadow: 0 0 20px rgba(0, 0, 0, 0.15);">';
    table += '<tr style="background-color: #EC7E46; color: #ffffff; text-align: left;">';
    table += '<th style="padding: 12px 15px;">Artifact id</th><th style="padding: 12px 15px;">AND id</th><th style="padding: 12px 15px;">ReadNodes</th><th style="padding: 12px 15px;">WriteNodes</th><th style="padding: 12px 15px;">KillNodes</th><th style="padding: 12px 15px;">Level</th><th style="padding: 12px 15px;">AnomalyType</th><th style="padding: 12px 15px;">Code</th></tr>';

    // Loop through each data item and add table rows
    data.forEach(item => {
      table += `<tr style="border-bottom: 1px solid #dddddd;">
                <td style="padding: 12px 15px;">${item.Artifact_id || ''}</td>
                <td style="padding: 12px 15px;">${item.AndNode_Id || ''}</td>
                <td style="padding: 12px 15px;">${item.ReadNodes ? item.ReadNodes.join(', ') : ''}</td>
                <td style="padding: 12px 15px;">${item.WriteNodes ? item.WriteNodes.join(', ') : ''}</td>
                <td style="padding: 12px 15px;">${item.KillNodes ? item.KillNodes.join(', ') : ''}</td>
                <td style="padding: 12px 15px;">${item.Level || ''}</td>
                <td style="padding: 12px 15px;">${item.AnomalyType || ''}</td>
                <td style="padding: 12px 15px;">${item.Code || ''}</td>   
              </tr>`;
    });

    table += '</table>';
    el.innerHTML = `<span style="float:right; cursor:pointer; color:#000;">&times;</span><div>${table}</div>`;

    var closeButton = el.querySelector('span');
    closeButton.style.marginRight = '10px';
    closeButton.style.marginTop = '5px';

    document.body.appendChild(el);

    closeButton.onclick = function() {
      document.body.removeChild(el);
    };
  }


  // PrevMethodRenderTable
  async PrevMethodRenderTable(data) {
    var el = document.createElement('div');
    el.setAttribute('style', `
    position: fixed; 
    bottom: 10%; 
    left: 50%; 
    transform: translateX(-50%);
    background-color: #fff; 
    color: #333; 
    padding: 20px; 
    border-radius: 5px; 
    box-shadow: 0px 0px 10px #999;
    z-index: 1000;
    min-width: 300px;
    max-width: 80%;
    box-sizing: border-box;
    overflow-x: auto;
    max-height: 33vh; /* 1/3 of the viewport height */
overflow-y: auto; /* Enables vertical scrolling */
  `);

    // Create table
    var table = document.createElement('table');
    table.setAttribute('style', `
    border-collapse: collapse;
    margin: 20px 0;
    font-size: 0.9em;
    min-width: 400px;
    width: 100%;
    box-shadow: 0 0 20px rgba(0, 0, 0, 0.15);
  `);
    var thead = document.createElement('thead');
    thead.setAttribute('style', `
    background-color: #f3f3f3; // darkRed, D8000C  green 009879 //redf FF0000  f3f3f3
    color: #FFFFFF;
    text-align: left;
  `);
    var tbody = document.createElement('tbody');
    tbody.setAttribute('style', `
    background-color: #f3f3f3;
  `);

    // Define column headers
    var headers = [ 'ArtifactId', 'Code', 'Type' , 'WriteNode', 'WriteNode2', 'ReadNode','KillNode' ];
    var tr = document.createElement('tr');
    headers.forEach(header => {
      var th = document.createElement('th');
      th.textContent = header;
      th.setAttribute('style', `
      padding: 12px 15px;
        color: #e75d18;
      border-right: 1px solid #dddddd;
    `);
      tr.appendChild(th);
    });
    thead.appendChild(tr);

    // Populate data rows
    data.forEach(item => {
      var tr = document.createElement('tr');
      tr.setAttribute('style', 'border-bottom: 1px solid #dddddd;');
      headers.forEach(header => {
        var td = document.createElement('td');
        td.textContent = item[header];
        td.setAttribute('style', 'padding: 12px 15px;');
        tr.appendChild(td);
      });
      tbody.appendChild(tr);
    });

    // Construct the table
    table.appendChild(thead);
    table.appendChild(tbody);
    el.appendChild(table);

    // Add a close button
    var closeButton = document.createElement('span');
    closeButton.textContent = '×';
    closeButton.setAttribute('style', `
    position: absolute;
    top: 10px;
    right: 20px;
    cursor: pointer;
    color: #000;
    font-size: 24px;
  `);

    // Append the close button and setup the click event
    el.appendChild(closeButton);
    closeButton.onclick = function() {
      document.body.removeChild(el);
    };

    // Append the whole element to the body
    document.body.appendChild(el);
  }

  async startInstance(processDefinition, options) {

    const {
      businessKey,
      variables
    } = options;

    const response = await this.fetch(`/process-definition/${processDefinition.id}/start`, {
      method: 'POST',
      body: JSON.stringify({
        businessKey,
        variables
      }),
      headers: {
        'content-type': 'application/json'
      }
    });

    if (response.ok) {
      return await response.json();
    }

    const body = await this.parse(response);

    throw new StartInstanceError(response, body);
  }

  async checkConnection() {

    const response = await this.fetch('/deployment?maxResults=0');

    if (response.ok) {
      return;
    }

    throw new ConnectionError(response);
  }

  async getVersion() {

    const response = await this.fetch('/version');

    if (response.ok) {
      const { version } = await response.json();
      return {
        version: version
      };
    }

    throw new ConnectionError(response);
  }

  getAuthentication(endpoint) {

    const {
      authType,
      username,
      password,
      token
    } = endpoint;

    switch (authType) {
    case AuthTypes.basic:
      return {
        username,
        password
      };
    case AuthTypes.bearer:
      return {
        token
      };
    }
  }

  getHeaders() {
    const headers = {
      accept: 'application/json'
    };

    if (this.authentication) {
      headers.authorization = this.getAuthHeader(this.authentication);
    }

    return headers;
  }

  getAuthHeader(endpoint) {

    const {
      token,
      username,
      password
    } = endpoint;

    if (token) {
      return `Bearer ${token}`;
    }

    if (username && password) {
      const credentials = window.btoa(`${username}:${password}`);

      return `Basic ${credentials}`;
    }
  }

  async fetch(path, options = {}) {
    const url = `${this.baseUrl}${path}`;
    const headers = {
      ...options.headers,
      ...this.getHeaders()
    };

    try {
      const signal = options.signal || this.setupTimeoutSignal();

      return await fetch(url, {
        ...options,
        headers,
        signal
      });
    } catch (error) {
      log('failed to fetch', error);

      return {
        url,
        json: () => {
          return {};
        }
      };
    }
  }

  async fetchAbs(path, options = {}) {
    const url = path;
    const headers = {
      ...options.headers,
      ...this.getHeaders()
    };

    try {
      const signal = options.signal || this.setupTimeoutSignal();

      return await this.fetchAbs(url, {
        ...options,
        headers,
        signal
      });
    } catch (error) {
      log('failed to fetch', error);

      return {
        url,
        json: () => {
          return {};
        }
      };
    }
  }

  setupTimeoutSignal(timeout = FETCH_TIMEOUT) {
    const controller = new AbortController();

    setTimeout(() => controller.abort(), timeout);

    return controller.signal;
  }

  async parse(response) {
    try {
      const json = await response.json();

      return json;
    } catch (error) {
      return {};
    }
  }
}

const NO_INTERNET_CONNECTION = 'NO_INTERNET_CONNECTION';
const CONNECTION_FAILED = 'CONNECTION_FAILED';
const DIAGRAM_PARSE_ERROR = 'DIAGRAM_PARSE_ERROR';
const UNAUTHORIZED = 'UNAUTHORIZED';
const FORBIDDEN = 'FORBIDDEN';
const NOT_FOUND = 'NOT_FOUND';
const INTERNAL_SERVER_ERROR = 'INTERNAL_SERVER_ERROR';
const UNAVAILABLE_ERROR = 'UNAVAILABLE_ERROR';
const CONCURRENT_ANOMALY = 'CONCURRENT_ANOMALY';

export const ApiErrors = {
  NO_INTERNET_CONNECTION,
  CONNECTION_FAILED,
  DIAGRAM_PARSE_ERROR,
  UNAUTHORIZED,
  FORBIDDEN,
  NOT_FOUND,
  INTERNAL_SERVER_ERROR,
  UNAVAILABLE_ERROR
};

export const ApiErrorMessages = {
  [ NO_INTERNET_CONNECTION ]: 'Could not establish a network connection.',
  [ CONNECTION_FAILED ]: 'Should point to a running Camunda REST API.',
  [ DIAGRAM_PARSE_ERROR ]: 'Server could not parse the diagram. Please check log for errors.',
  [ UNAUTHORIZED ]: 'Credentials do not match with the server.',
  [ FORBIDDEN ]: 'This user is not permitted to deploy. Please use different credentials or get this user enabled to deploy.',
  [ NOT_FOUND ]: 'Should point to a running Camunda REST API.',
  [ INTERNAL_SERVER_ERROR ]: 'Camunda is reporting an error. Please check the server status.',
  [ UNAVAILABLE_ERROR ]: 'Camunda is reporting an error. Please check the server status.',
  [CONCURRENT_ANOMALY ]: 'Artifact Anomaly detector is reporting an error. Please ceck the log for details.'
};

export class ConnectionError extends Error {

  constructor(response) {
    super('Connection failed');

    this.code = (
      getResponseErrorCode(response) ||
      getNetworkErrorCode(response)
    );

    this.details = ApiErrorMessages[this.code];
  }
}


export class DeploymentError extends Error {

  constructor(response, body) {
    super('Deployment failed');

    this.code = (
      getCamundaErrorCode(response, body) ||
      getResponseErrorCode(response) ||
      getNetworkErrorCode(response)
    );

    this.details = ApiErrorMessages[this.code];

    this.problems = body && body.message;
  }
}

export class AnomalyError extends Error {

  constructor(detials, problems) {
    super('Anomaly free failed');

    this.code = 'concurrent Anomaly';

    this.details = detials;

    this.problems = problems ;
  }
}

export class StartInstanceError extends Error {

  constructor(response, body) {
    super('Starting instance failed');

    this.code = (
      getCamundaErrorCode(response, body) ||
      getResponseErrorCode(response) ||
      getNetworkErrorCode(response)
    );

    this.details = ApiErrorMessages[this.code];

    this.problems = body && body.message;
  }
}


// helpers ///////////////

function getNetworkErrorCode(response) {
  if (isLocalhost(response.url) || isOnline()) {
    return CONNECTION_FAILED;
  }

  return NO_INTERNET_CONNECTION;
}

function getResponseErrorCode(response) {
  switch (response.status) {
  case 401:
    return UNAUTHORIZED;
  case 403:
    return FORBIDDEN;
  case 404:
    return NOT_FOUND;
  case 500:
    return INTERNAL_SERVER_ERROR;
  case 503:
      return UNAVAILABLE_ERROR;
    case 601:
      return CONCURRENT_ANOMALY;
  }
}

function getCamundaErrorCode(response, body) {

  const PARSE_ERROR_PREFIX = 'ENGINE-09005 Could not parse BPMN process.';

  if (body && body.message && body.message.startsWith(PARSE_ERROR_PREFIX)) {
    return DIAGRAM_PARSE_ERROR;
  }
}

function isLocalhost(url) {
  return /^https?:\/\/(127\.0\.0\.1|localhost)/.test(url);
}

function isOnline() {
  return window.navigator.onLine;
}

function normalizeBaseURL(url) {
  return url.replace(/\/deployment\/create\/?/, '');
}
