import ThreeGlobe from "three-globe";
import * as THREE from "three";
import {OrbitControls} from "three/examples/jsm/controls/OrbitControls.js";
import * as countries from "./country_data.json";
import * as travelHistory from "./my-flights.json";
import * as airportHistory from "./my-airports.json";

export class GlobeContext {
  private readonly _containerElement: HTMLCanvasElement;
  private readonly _renderer: THREE.WebGLRenderer;
  private readonly _scene: THREE.Scene;
  private readonly _camera: THREE.PerspectiveCamera;
  private readonly _controls: OrbitControls;
  private readonly _globe: ThreeGlobe;

  public constructor(containerElement: HTMLCanvasElement) {
    this._containerElement = containerElement;

    // init renderer
    this._renderer = new THREE.WebGLRenderer({antialias: true});
    this._renderer.setPixelRatio(window.devicePixelRatio);
    this._renderer.setSize(window.innerWidth, window.innerHeight);
    this._containerElement.appendChild(this._renderer.domElement);

    // init scene
    this._scene = new THREE.Scene();
    this._scene.add(new THREE.AmbientLight(0xbbbbbb, 0.3));
    this._scene.background = new THREE.Color(0x040d21);
    this._scene.fog = new THREE.Fog(0x535ef3, 400, 2000);

    // init camera
    this._camera = new THREE.PerspectiveCamera();
    this._camera.aspect = window.innerWidth / window.innerHeight;
    this._camera.updateProjectionMatrix();
    this._camera.position.z = 400;
    this._camera.position.x = 0;
    this._camera.position.y = 0;

    // add camera to the scene
    this._scene.add(this._camera);

    // init lights
    const dLight = new THREE.DirectionalLight(0xffffff, 0.8);
    const dLight1 = new THREE.DirectionalLight(0x7982f6, 1);
    const pLight = new THREE.PointLight(0x8566cc, 0.5);

    dLight.position.set(-800, 2000, 400);
    dLight1.position.set(-200, 500, 200);
    pLight.position.set(-200, 500, 200);

    this._camera.add(dLight);
    this._camera.add(dLight1);
    this._camera.add(pLight);

    // init helpers
    // const axesHelper = new AxesHelper(800);
    // const helper = new DirectionalLightHelper(dLight);
    // const helperCamera = new CameraHelper(dLight.shadow.camera);
    // this._scene.add(axesHelper);
    // this._scene.add(helper);
    // this._scene.add(helperCamera);

    // init controls
    this._controls = new OrbitControls(this._camera, this._renderer.domElement);
    this._controls.enableDamping = true;
    this._controls.dampingFactor = 0.01;
    this._controls.enablePan = false;
    this._controls.minDistance = 200;
    this._controls.maxDistance = 500;
    this._controls.rotateSpeed = 0.8;
    this._controls.zoomSpeed = 1;
    this._controls.autoRotate = false;
    this._controls.minPolarAngle = Math.PI / 3.5;
    this._controls.maxPolarAngle = Math.PI - Math.PI / 3;

    this._globe = GlobeContext.createGlobe();
    this._scene.add(this._globe);

    window.addEventListener("resize", () => this.onWindowResize(), false);
  }

  private static createGlobe(): ThreeGlobe {
    const globe = new ThreeGlobe({
      waitForGlobeReady: true,
      animateIn: true,
    })
      .hexPolygonsData(countries.features)
      .hexPolygonResolution(3)
      .hexPolygonMargin(0.7)
      .showAtmosphere(true)
      .atmosphereColor("#3a228a")
      .atmosphereAltitude(0.25)
      .hexPolygonColor((e: { properties: { ISO_A3: string } }) => {
        if (["KGZ", "KOR", "THA", "RUS", "UZB", "IDN", "KAZ", "MYS"].includes(e.properties.ISO_A3)) {
          return "rgba(255,255,255, 1)";
        } else
          return "rgba(255,255,255, 0.7)";
      });

    // NOTE Arc animations are followed after the globe enters the scene
    setTimeout(() => {
      globe.arcsData(travelHistory.flights)
        .arcColor((e: { status: boolean; }) => e.status ? "#9cff00" : "#FF4000")
        .arcAltitude((e: { arcAlt: number }) => e.arcAlt)
        .arcStroke((e: { status: boolean }) => e.status ? 0.5 : 0.3)
        .arcDashLength(0.9)
        .arcDashGap(4)
        .arcDashAnimateTime(1000)
        .arcsTransitionDuration(1000)
        .arcDashInitialGap((e: { order: number }) => e.order)
        .labelsData(airportHistory.airports)
        .labelColor(() => "#ffcb21")
        .labelDotOrientation((e: { text: string }) => e.text === "ALA" ? "top" : "right")
        .labelDotRadius(0.3)
        .labelSize((e: { size: number }) => e.size)
        .labelText("city")
        .labelResolution(6)
        .labelAltitude(0.01)
        .pointsData(airportHistory.airports)
        .pointColor(() => "#ffffff")
        .pointsMerge(true)
        .pointAltitude(0.07)
        .pointRadius(0.05);
    }, 1000);

    globe.rotateY(-Math.PI * (5 / 9));
    globe.rotateZ(-Math.PI / 6);

    const globeMaterial = globe.globeMaterial() as THREE.Material;
    // @ts-ignore
    globeMaterial.color = new THREE.Color(0x3a228a);
    // @ts-ignore
    globeMaterial.emissive = new THREE.Color(0x220038);
    // @ts-ignore
    globeMaterial.emissiveIntensity = 0.1;
    // @ts-ignore
    globeMaterial.shininess = 0.7;
    // @ts-ignore
    globeMaterial.wireframe = true;

    return globe;
  }

  private onWindowResize() {
    this._camera.aspect = window.innerWidth / window.innerHeight;
    this._camera.updateProjectionMatrix();
    this._renderer.setSize(window.innerWidth, window.innerHeight);
  }

  private animate() {
    this._camera.lookAt(this._scene.position);
    this._controls.update();
    this._renderer.render(this._scene, this._camera);
    requestAnimationFrame(() => this.animate());
  }

  public start() {
    this.onWindowResize();
    this.animate();
  }
}

const el = document.querySelector("#globeContainer")
const globe = new GlobeContext(el as HTMLCanvasElement);
globe.start();